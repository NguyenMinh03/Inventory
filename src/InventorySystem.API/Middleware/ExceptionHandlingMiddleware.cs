using FluentValidation;
using InventorySystem.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.API.Middleware;

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
        var (statusCode, title, errors) = exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                "Validation failed.",
                (IDictionary<string, string[]>?)validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),

            // Order matters: InsufficientStockException and AuthenticationException
            // both derive from DomainException, so their arms must come first to
            // get their own status codes.
            InsufficientStockException stockEx => (StatusCodes.Status409Conflict, stockEx.Message, null),
            AuthenticationException authEx => (StatusCodes.Status401Unauthorized, authEx.Message, null),
            DomainException domainEx => (StatusCodes.Status400BadRequest, domainEx.Message, null),
            KeyNotFoundException notFoundEx => (StatusCodes.Status404NotFound, notFoundEx.Message, null),

            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", null),
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
        else
            _logger.LogWarning("Handled exception processing {Method} {Path}: {Message}", context.Request.Method, context.Request.Path, exception.Message);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Instance = context.Request.Path,
        };

        if (errors is not null)
            problem.Extensions["errors"] = errors;

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(problem);
    }
}
