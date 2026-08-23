using System;
using System.Threading.Tasks;
using hihapi.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<ErrorHandlingMiddleware> logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task Invoke(HttpContext context)
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

    private Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var code = StatusCodes.Status500InternalServerError;

        if (ex is NotFoundException) code = StatusCodes.Status404NotFound;
        else if (ex is UnauthorizedException
            || ex is UnauthorizedAccessException) code = StatusCodes.Status401Unauthorized;
        else if (ex is BadRequestException) code = StatusCodes.Status400BadRequest;
        else if (ex is DBOperationException) code = StatusCodes.Status400BadRequest;

        if (code == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(ex, "Unhandled exception occurred");
        }
        else
        {
            logger.LogWarning(ex, "Handled exception: {ExceptionType}", ex.GetType().Name);
        }

        // For 500s, return a generic message and rely on the server-side log above
        // (line 42) for details; never leak ex.Message for unhandled exceptions.
        // For explicitly-handled 4xx types, ex.Message is intentional (e.g. validation).
        var message = code == StatusCodes.Status500InternalServerError
            ? "An internal server error occurred."
            : ex.Message;
        var result = System.Text.Json.JsonSerializer.Serialize(new { error = message });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        return context.Response.WriteAsync(result);
    }
}
