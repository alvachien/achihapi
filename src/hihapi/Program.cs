using System.Text.Json.Serialization;
using hihapi;
using hihapi.Models;
using hihapi.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);

// Config the log
builder.Host.UseSerilog((context, config) =>
{
    var environment = context.HostingEnvironment;
    var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}";

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
                 rollingInterval: RollingInterval.Day,
                 outputTemplate: outputTemplate,
                 retainedFileCountLimit: 14
             );
    }
});

// Ensure folders exist
HIHAPIUtility.UploadFolder = HIHAPIUtility.EnsureFolderExistence(builder.Environment.ContentRootPath, @"data/uploads");
HIHAPIUtility.BlogFolder = HIHAPIUtility.EnsureFolderExistence(builder.Environment.ContentRootPath, @"data/blogs");

// Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultDB");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<hihDataContext>(options => options.UseSqlite(connectionString));
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

// Read auth/CORS config from appsettings
var identityServerUrl = builder.Configuration["Auth:IdentityServerUrl"] ?? "https://localhost:44353";
var jwtAudience = builder.Configuration["Auth:JwtAudience"] ?? "api.hih";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "https://localhost:29521", "https://localhost:29528", "https://localhost:29525" };

const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            options.Authority = identityServerUrl;
            options.RequireHttpsMetadata = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = jwtAudience
            };
        });
}
else if (builder.Environment.IsProduction())
{
    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            options.Authority = identityServerUrl;
            options.RequireHttpsMetadata = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = jwtAudience
            };
        });
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins, builder =>
    {
        builder.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
            .AllowCredentials();
    });
});

// Fallback authorization policy — all endpoints require auth by default
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Response Caching
builder.Services.AddResponseCaching();
// Memory cache
builder.Services.AddMemoryCache();
// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Seed reference data on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<hihDataContext>();
    await DatabaseSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
}

app.UseSerilogRequestLogging();

// Security headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    await next();
});

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseODataBatching();

app.UseResponseCaching();

app.UseRouting();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();
