using System.Net;
using System.Text.Json;
using FrameRent.Domain.Exceptions;

namespace FrameRent.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            DomainValidationException => (HttpStatusCode.BadRequest, "Validation Error"),
            InvalidOperationException => (HttpStatusCode.BadRequest, "Business Rule Violation"),
            NotFoundException => (HttpStatusCode.NotFound, "Resource Not Found"),
            ForbiddenAccessException => (HttpStatusCode.Forbidden, "Access Denied"),
            ConflictException => (HttpStatusCode.Conflict, "Conflict"),
            _ => (HttpStatusCode.InternalServerError, "Server Error")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred.");
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            error = title,
            message = statusCode == HttpStatusCode.InternalServerError
                ? "An unexpected error occurred while processing your request."
                : exception.Message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
