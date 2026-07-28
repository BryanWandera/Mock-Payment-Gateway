using System.Net;
using System.Text.Json;
using PaymentMock.DTOs.Generic;
using PaymentMock.Exceptions;
using PaymentMock.Models;
using PaymentMock.Repositories.Interfaces;

namespace PaymentMock.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuditLogRepository auditLogRepository)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.Items["CorrelationId"] as string ?? Guid.NewGuid().ToString();
            var (statusCode, errorCode, message, details) = MapException(ex);

            _logger.LogError(ex, "Unhandled exception for {Method} {Path} [{CorrelationId}]",
                context.Request.Method, context.Request.Path, correlationId);

            try
            {
                await auditLogRepository.CreateAsync(new AuditLog
                {
                    LogType = "Error",
                    Message = message,
                    Details = ex.ToString(),
                    CorrelationId = correlationId
                });
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Failed to persist audit log entry");
            }

            if (context.Response.HasStarted)
                return;

            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new ErrorResponseDto
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode,
                Details = details
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
    }

    private static (HttpStatusCode StatusCode, string ErrorCode, string Message, object? Details) MapException(Exception ex) => ex switch
    {
        ValidationException validationEx => (HttpStatusCode.BadRequest, "VALIDATION_ERROR", "Request validation failed", validationEx.Errors),
        NotFoundException notFoundEx => (HttpStatusCode.NotFound, "NOT_FOUND", notFoundEx.Message, null),
        ConflictException conflictEx => (HttpStatusCode.Conflict, "CONFLICT", conflictEx.Message, null),
        GatewayUnauthorizedException unauthorizedEx => (HttpStatusCode.Unauthorized, "UNAUTHORIZED", unauthorizedEx.Message, null),
        _ => (HttpStatusCode.InternalServerError, "GATEWAY_ERROR", "An unexpected error occurred while processing the request", null)
    };
}
