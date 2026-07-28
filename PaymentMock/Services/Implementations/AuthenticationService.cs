using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using PaymentMock.Configuration;
using PaymentMock.Exceptions;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Services.Implementations;

public class AuthenticationService : IAuthenticationService
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(5);

    private readonly ConcurrentDictionary<string, DateTime> _tokens = new();
    private readonly AuthenticationSettings _authSettings;

    public AuthenticationService(IOptions<AppSettings> options)
    {
        _authSettings = options.Value.Authentication;
    }

    public Task<(string Token, DateTime ExpiresAt)> IssueTokenAsync(string consumerKey, string consumerSecret)
    {
        var isValid = _authSettings.PesapalConsumerKeys.Any(k =>
            k.ConsumerKey == consumerKey && k.ConsumerSecret == consumerSecret);

        if (!isValid)
            throw new GatewayUnauthorizedException("Invalid consumer_key/consumer_secret pair");

        var token = Guid.NewGuid().ToString("N");
        var expiresAt = DateTime.UtcNow.Add(TokenLifetime);
        _tokens[token] = expiresAt;

        return Task.FromResult((token, expiresAt));
    }

    public Task<bool> ValidateBearerTokenAsync(string token)
    {
        if (_tokens.TryGetValue(token, out var expiresAt))
        {
            if (expiresAt > DateTime.UtcNow)
                return Task.FromResult(true);

            _tokens.TryRemove(token, out _);
        }

        return Task.FromResult(false);
    }
}
