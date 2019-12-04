using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using hihapi.Exceptions;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext context /* other dependencies */)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var code = StatusCodes.Status500InternalServerError; // 500 if unexpected

        if      (ex is NotFoundException)     code = StatusCodes.Status404NotFound;
        else if (ex is UnauthorizedException
            || ex is UnauthorizedAccessException) code = StatusCodes.Status401Unauthorized;
        else if (ex is BadRequestException)   code = StatusCodes.Status400BadRequest;

        var result = JsonConvert.SerializeObject(new { error = ex.Message });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        return context.Response.WriteAsync(result);
    }
}
