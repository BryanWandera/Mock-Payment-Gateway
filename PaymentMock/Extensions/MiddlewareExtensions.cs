using PaymentMock.Middleware;

namespace PaymentMock.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UsePaymentMockMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
        app.UseMiddleware<PesapalBearerAuthenticationMiddleware>();
        app.UseMiddleware<IdempotencyMiddleware>();
        return app;
    }
}
