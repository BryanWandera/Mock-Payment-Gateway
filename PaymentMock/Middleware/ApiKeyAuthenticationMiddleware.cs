using System.Text.Json;
using Microsoft.Extensions.Options;
using PaymentMock.Configuration;
using PaymentMock.DTOs.Generic;
using PaymentMock.Enums;

namespace PaymentMock.Middleware;

public class ApiKeyAuthenticationMiddleware
{
    private const string ApiKeyHeader = "X-API-Key";

    private static readonly string[] AnonymousPathPrefixes =
    {
        "/health", "/swagger", "/api/v1/auth/token", "/checkout"
    };

    private readonly RequestDelegate _next;

    public ApiKeyAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IOptions<AppSettings> options)
    {
        var settings = options.Value;

        if (!Enum.TryParse<GatewayProfileType>(settings.GatewayProfile, true, out var profile) || profile != GatewayProfileType.Generic)
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (AnonymousPathPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var providedKey = context.Request.Headers[ApiKeyHeader].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(providedKey))
        {
            await WriteUnauthorized(context, "Missing X-API-Key header");
            return;
        }

        var isValid = settings.Authentication.ApiKeys.Any(k => k.Key == providedKey);
        if (!isValid)
        {
            await WriteUnauthorized(context, "Invalid API key");
            return;
        }

        await _next(context);
    }

    private static async Task WriteUnauthorized(HttpContext context, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        var response = new ErrorResponseDto
        {
            Success = false,
            Message = message,
            ErrorCode = "UNAUTHORIZED"
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
