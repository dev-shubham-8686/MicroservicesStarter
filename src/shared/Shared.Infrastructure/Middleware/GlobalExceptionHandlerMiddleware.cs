using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using Shared.Contracts.Common;
using Shared.Infrastructure.Exceptions;

namespace Shared.Infrastructure.Middleware;

/// <summary>
/// Global exception handling middleware for consistent error responses
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
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
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
            ?? context.TraceIdentifier;

        context.Response.ContentType = "application/json";

        var (errorResponse, statusCode) = exception switch
        {
            ValidationException validationEx => (HandleValidationException(context, validationEx, correlationId), 400),
            NotFoundException notFoundEx => (HandleNotFoundException(context, notFoundEx, correlationId), 404),
            ConflictException conflictEx => (HandleConflictException(context, conflictEx, correlationId), 409),
            UnauthorizedException unauthorizedEx => (HandleUnauthorizedException(context, unauthorizedEx, correlationId), 401),
            ForbiddenException forbiddenEx => (HandleForbiddenException(context, forbiddenEx, correlationId), 403),
            BusinessRuleException businessEx => (HandleBusinessRuleException(context, businessEx, correlationId), 422),
            AppException appEx => (HandleApplicationException(context, appEx, correlationId), appEx.StatusCode),
            _ => (HandleUnknownException(context, exception, correlationId), 500)
        };

        context.Response.StatusCode = statusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }

    private ApiResponse<object> HandleValidationException(
        HttpContext context,
        ValidationException exception,
        string correlationId)
    {
        _logger.LogWarning(exception, "Validation error occurred. CorrelationId: {CorrelationId}", correlationId);

        return new ApiResponse<object>
        {
            Success = false,
            Error = new ErrorResponse
            {
                ErrorCode = exception.ErrorCode,
                Message = exception.Message,
                Details = _environment.IsDevelopment() ? exception.StackTrace : null,
                ValidationErrors = exception.ValidationErrors,
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId
            }
        };
    }

    private ApiResponse<object> HandleNotFoundException(
        HttpContext context,
        NotFoundException exception,
        string correlationId)
    {
        _logger.LogWarning(exception, "Resource not found. CorrelationId: {CorrelationId}", correlationId);

        return new ApiResponse<object>
        {
            Success = false,
            Error = new ErrorResponse
            {
                ErrorCode = exception.ErrorCode,
                Message = exception.Message,
                Details = _environment.IsDevelopment() ? exception.StackTrace : null,
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId
            }
        };
    }

    private ApiResponse<object> HandleConflictException(
        HttpContext context,
        ConflictException exception,
        string correlationId)
    {
        _logger.LogWarning(exception, "Conflict occurred. CorrelationId: {CorrelationId}", correlationId);

        return new ApiResponse<object>
        {
            Success = false,
            Error = new ErrorResponse
            {
                ErrorCode = exception.ErrorCode,
                Message = exception.Message,
                Details = _environment.IsDevelopment() ? exception.StackTrace : null,
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId
            }
        };
    }

    private ApiResponse<object> HandleUnauthorizedException(
        HttpContext context,
        UnauthorizedException exception,
        string correlationId)
    {
        _logger.LogWarning(exception, "Unauthorized access. CorrelationId: {CorrelationId}", correlationId);

        return new ApiResponse<object>
        {
            Success = false,
            Error = new ErrorResponse
            {
                ErrorCode = exception.ErrorCode,
                Message = exception.Message,
                Details = _environment.IsDevelopment() ? exception.StackTrace : null,
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId
            }
        };
    }

    private ApiResponse<object> HandleForbiddenException(
        HttpContext context,
        ForbiddenException exception,
        string correlationId)
    {
        _logger.LogWarning(exception, "Forbidden access. CorrelationId: {CorrelationId}", correlationId);

        return new ApiResponse<object>
        {
            Success = false,
            Error = new ErrorResponse
            {
                ErrorCode = exception.ErrorCode,
                Message = exception.Message,
                Details = _environment.IsDevelopment() ? exception.StackTrace : null,
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId
            }
        };
    }

    private ApiResponse<object> HandleBusinessRuleException(
        HttpContext context,
        BusinessRuleException exception,
        string correlationId)
    {
        _logger.LogWarning(exception, "Business rule violation. CorrelationId: {CorrelationId}", correlationId);

        return new ApiResponse<object>
        {
            Success = false,
            Error = new ErrorResponse
            {
                ErrorCode = exception.ErrorCode,
                Message = exception.Message,
                Details = _environment.IsDevelopment() ? exception.StackTrace : null,
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId
            }
        };
    }

    private ApiResponse<object> HandleApplicationException(
        HttpContext context,
        AppException exception,
        string correlationId)
    {
        _logger.LogError(exception, "Application error occurred. CorrelationId: {CorrelationId}", correlationId);

        return new ApiResponse<object>
        {
            Success = false,
            Error = new ErrorResponse
            {
                ErrorCode = exception.ErrorCode,
                Message = exception.Message,
                Details = _environment.IsDevelopment() ? exception.StackTrace : null,
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId
            }
        };
    }

    private ApiResponse<object> HandleUnknownException(
        HttpContext context,
        Exception exception,
        string correlationId)
    {
        _logger.LogError(exception, "Unhandled exception occurred. CorrelationId: {CorrelationId}", correlationId);

        return new ApiResponse<object>
        {
            Success = false,
            Error = new ErrorResponse
            {
                ErrorCode = ErrorCodes.InternalServerError,
                Message = "An unexpected error occurred. Please try again later.",
                Details = _environment.IsDevelopment() ? exception.ToString() : null,
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId
            }
        };
    }
}

public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}

