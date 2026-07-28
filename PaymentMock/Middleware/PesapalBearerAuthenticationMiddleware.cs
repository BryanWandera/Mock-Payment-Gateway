using System.Text.Json;
using Microsoft.Extensions.Options;
using PaymentMock.Configuration;
using PaymentMock.DTOs.Generic;
using PaymentMock.Enums;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Middleware;

public class PesapalBearerAuthenticationMiddleware
{
    private static readonly string[] AnonymousPathPrefixes =
    {
        "/health", "/swagger", "/api/v1/auth/token", "/checkout"
    };

    private readonly RequestDelegate _next;

    public PesapalBearerAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IOptions<AppSettings> options, IAuthenticationService authenticationService)
    {
        var settings = options.Value;

        if (!Enum.TryParse<GatewayProfileType>(settings.GatewayProfile, true, out var profile) || profile != GatewayProfileType.Pesapal)
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

        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            await WriteUnauthorized(context, "Missing bearer token. Obtain one from /api/v1/auth/token");
            return;
        }

        var token = authHeader["Bearer ".Length..].Trim();
        var isValid = await authenticationService.ValidateBearerTokenAsync(token);
        if (!isValid)
        {
            await WriteUnauthorized(context, "Invalid or expired bearer token");
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
