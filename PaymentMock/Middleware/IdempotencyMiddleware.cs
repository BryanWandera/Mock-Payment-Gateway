using System.Security.Cryptography;
using System.Text;
using PaymentMock.Exceptions;
using PaymentMock.Models;
using PaymentMock.Repositories.Interfaces;

namespace PaymentMock.Middleware;

public class IdempotencyMiddleware
{
    private const string IdempotencyKeyHeader = "Idempotency-Key";
    private static readonly TimeSpan RecordLifetime = TimeSpan.FromHours(24);

    private static readonly string[] IdempotentPathPrefixes =
    {
        "/api/v1/payments", "/api/v1/payouts"
    };

    private readonly RequestDelegate _next;

    public IdempotencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IIdempotencyRepository idempotencyRepository)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var isEligible = context.Request.Method == HttpMethods.Post &&
                          IdempotentPathPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        var idempotencyKey = context.Request.Headers[IdempotencyKeyHeader].FirstOrDefault();

        if (!isEligible || string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await _next(context);
            return;
        }

        context.Request.EnableBuffering();
        using var requestReader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
        var requestBody = await requestReader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        var requestHash = ComputeHash(requestBody);

        var existing = await idempotencyRepository.GetByKeyAndPathAsync(idempotencyKey, path);
        if (existing != null)
        {
            if (existing.RequestHash != requestHash)
                throw new ConflictException("Idempotency-Key has already been used with a different request payload");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = existing.StatusCode;
            await context.Response.WriteAsync(existing.ResponseBody);
            return;
        }

        var originalBody = context.Response.Body;
        await using var bufferStream = new MemoryStream();
        context.Response.Body = bufferStream;

        await _next(context);

        bufferStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(bufferStream).ReadToEndAsync();
        bufferStream.Seek(0, SeekOrigin.Begin);
        await bufferStream.CopyToAsync(originalBody);
        context.Response.Body = originalBody;

        if (context.Response.StatusCode is >= 200 and < 300)
        {
            await idempotencyRepository.CreateAsync(new IdempotencyRecord
            {
                IdempotencyKey = idempotencyKey,
                RequestPath = path,
                RequestHash = requestHash,
                StatusCode = context.Response.StatusCode,
                ResponseBody = responseBody,
                ExpiresAt = DateTime.UtcNow.Add(RecordLifetime)
            });
        }
    }

    private static string ComputeHash(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes);
    }
}
