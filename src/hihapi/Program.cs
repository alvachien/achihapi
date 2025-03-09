using hihapi.Models;
using hihapi;
using hihapi.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Config the log
builder.Host.UseSerilog((context, config) =>
{
    var environment = context.HostingEnvironment;
    var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}";

    //config.MinimumLevel.Is(environment.IsDevelopment() ? LogEventLevel.Information : LogEventLevel.Warning)
    //     .Enrich.FromLogContext()
    //     .WriteTo.File(
    //         path: "../Logs/ACIDServer/log-.txt",
    //         rollingInterval: RollingInterval.Day, // 按天滚动
    //         outputTemplate: outputTemplate,
    //         retainedFileCountLimit: 14 // 保留最近7天日志
    //     );
    if (environment.IsDevelopment())
    {
        config.MinimumLevel.Is(LogEventLevel.Information)
             .Enrich.FromLogContext()
             .WriteTo.Console(theme: SystemConsoleTheme.Colored);

    }
    else if (environment.IsProduction())
    {
        config.MinimumLevel.Is(LogEventLevel.Warning)
             .Enrich.FromLogContext()
             .WriteTo.File(
                 path: "../Logs/hihapi/log-.txt",
                 rollingInterval: RollingInterval.Day, // 按天滚动
                 outputTemplate: outputTemplate,
                 retainedFileCountLimit: 14 // 保留最近7天日志
             );
    }
});

// Ensure
HIHAPIUtility.UploadFolder = HIHAPIUtility.EnsureFolderExistence(builder.Environment.ContentRootPath, @"data/uploads");
HIHAPIUtility.BlogFolder = HIHAPIUtility.EnsureFolderExistence(builder.Environment.ContentRootPath, @"data/blogs");

// Connection string
var connectionString = string.Empty;
if (builder.Environment.IsDevelopment())
{
    connectionString = builder.Configuration.GetConnectionString("DefaultDB");
}
else if (builder.Environment.IsProduction())
{
    connectionString = builder.Configuration.GetConnectionString("DefaultDB");
}
if (connectionString.Length > 0)
{
    if (connectionString.EndsWith(';'))
        connectionString += "Encrypt=False;";
    else
        connectionString += ";Encrypt=False;";

    builder.Services.AddDbContext<hihDataContext>(options => options.UseSqlServer(connectionString));
}

builder.Services.AddHttpContextAccessor();

// OData Edm Model
IEdmModel model = EdmModelBuilder.GetEdmModel();

builder.Services.AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .AddOData(opt => opt.Count().Filter().Expand().Select().OrderBy().SetMaxTop(100)
    .AddRouteComponents(model)
    .AddRouteComponents("v1", model)
    );

const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
if (builder.Environment.IsDevelopment())
{
    // accepts any access token issued by identity server
    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            options.Authority = "https://localhost:44353";
            options.RequireHttpsMetadata = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false
            };
        });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(MyAllowSpecificOrigins, builder =>
        {
            builder.WithOrigins(
                "https://localhost:29521",  // AC HIH UI
                "https://localhost:29528",  // AC HIH App
                "https://localhost:29525"   // acblog
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
    });
}
else if (builder.Environment.IsProduction())
{
    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            options.Authority = "https://www.alvachien.com/idserver";
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.IncludeErrorDetails = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false
            };

            options.Audience = "api.hih";
        });
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(MyAllowSpecificOrigins, builder =>
        {
            builder.WithOrigins(
                "https://www.alvachien.com/hih",
                "https://www.alvachien.com/alvablog",
                "https://www.alvachien.com/fishblog"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
    });
}

builder.Services.AddAuthorization();

// Response Caching
builder.Services.AddResponseCaching();
// Memory cache
builder.Services.AddMemoryCache();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCors(MyAllowSpecificOrigins);

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

app.UseODataBatching();

app.UseRouting()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    }); ;

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware(typeof(ErrorHandlingMiddleware));

app.UseResponseCaching();

//var cachePeriod = app.Environment.IsDevelopment() ? "10" : "30";
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(BlogFolder),
//    RequestPath = "/blogs",
//    OnPrepareResponse = ctx =>
//    {
//        // Requires the following import:
//        ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cachePeriod}");
//    }
//});

app.Run();

