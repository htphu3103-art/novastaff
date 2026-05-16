// Web/Middlewares/GlobalExceptionMiddleware.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using NovaStaff.Models.Exceptions;
using System.Net;

namespace NovaStaff.Web.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Response already started, skipping exception handling.");
            return;
        }

        var statusCode = ex switch
        {
            AppException appEx => appEx.StatusCode,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };

        // 500 → Error, còn lại → Warning (business rule violation không phải lỗi hệ thống)
        if (statusCode >= 500)
            _logger.LogError(ex,
                "Unhandled exception at {Path}. StatusCode: {StatusCode}. TraceId: {TraceId}",
                context.Request.Path, statusCode, context.TraceIdentifier);
        else
            _logger.LogWarning(ex,
                "Business exception at {Path}. StatusCode: {StatusCode}. TraceId: {TraceId}",
                context.Request.Path, statusCode, context.TraceIdentifier);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = _env.IsDevelopment()
                ? ex.Message
                : statusCode < 500
                    ? ex.Message       
                    : "An unexpected error occurred.",
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"]   = context.TraceIdentifier,
                ["errorCode"] = ex is AppException
                    ? ex.GetType().Name
                    : "INTERNAL_SERVER_ERROR"
            }
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem, context.RequestAborted);
    }
}
