namespace PaymentMock.Services.Interfaces;

public interface IAuthenticationService
{
    Task<(string Token, DateTime ExpiresAt)> IssueTokenAsync(string consumerKey, string consumerSecret);
    Task<bool> ValidateBearerTokenAsync(string token);
}
