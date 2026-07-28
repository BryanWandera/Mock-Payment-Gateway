using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using PaymentMock.Configuration;
using PaymentMock.DTOs.Generic;
using PaymentMock.Enums;
using PaymentMock.Exceptions;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Services.Implementations;

public partial class ValidationService : IValidationService
{
    private static readonly PaymentProvider[] MobileMoneyProviders = { PaymentProvider.MTN, PaymentProvider.Airtel };
    private static readonly PaymentProvider[] CardProviders = { PaymentProvider.Visa, PaymentProvider.Mastercard };

    private readonly GatewaySettings _gatewaySettings;

    public ValidationService(IOptions<AppSettings> options)
    {
        _gatewaySettings = options.Value.Gateway;
    }

    public void ValidateMobileMoneyPayment(MobileMoneyPaymentRequest request)
    {
        var errors = new List<string>();

        ValidateCommon(errors, request.Amount, request.Currency, request.MerchantReference);

        if (!MobileMoneyProviders.Contains(request.Provider))
            errors.Add($"Provider '{request.Provider}' is not a supported Mobile Money provider");

        if (string.IsNullOrWhiteSpace(request.PhoneNumber) || !PhoneNumberRegex().IsMatch(request.PhoneNumber))
            errors.Add("Phone number must be a valid MSISDN, e.g. +2567xxxxxxxx or 07xxxxxxxx");

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }

    public void ValidateCardPayment(CardPaymentRequest request)
    {
        var errors = new List<string>();

        ValidateCommon(errors, request.Amount, request.Currency, request.MerchantReference);

        if (!CardProviders.Contains(request.Provider))
            errors.Add($"Provider '{request.Provider}' is not a supported card scheme");

        var digitsOnly = DigitsOnlyRegex().Replace(request.CardNumber ?? string.Empty, string.Empty);
        if (!IsValidCardNumber(digitsOnly, request.Provider))
            errors.Add("Card number failed validation for the selected scheme");

        if (!int.TryParse(request.ExpiryMonth, out var month) || month is < 1 or > 12)
            errors.Add("Expiry month must be between 01 and 12");

        if (!int.TryParse(request.ExpiryYear, out var year))
        {
            errors.Add("Expiry year is invalid");
        }
        else
        {
            var normalizedYear = year < 100 ? 2000 + year : year;
            var expiry = new DateTime(normalizedYear, Math.Clamp(month, 1, 12), 1).AddMonths(1);
            if (expiry < DateTime.UtcNow)
                errors.Add("Card has expired");
        }

        if (string.IsNullOrWhiteSpace(request.Cvv) || !CvvRegex().IsMatch(request.Cvv))
            errors.Add("CVV must be 3 or 4 digits");

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }

    public void ValidateHostedCheckout(HostedCheckoutRequest request)
    {
        var errors = new List<string>();

        ValidateCommon(errors, request.Amount, request.Currency, request.MerchantReference);

        switch (request.PaymentMethod)
        {
            case PaymentMethod.Card:
                if (!CardProviders.Contains(request.Provider))
                    errors.Add($"Provider '{request.Provider}' is not a supported card scheme");
                break;
            case PaymentMethod.BankTransfer:
                if (request.Provider != PaymentProvider.Bank)
                    errors.Add($"Provider '{request.Provider}' is not valid for a bank transfer checkout");
                break;
            default:
                errors.Add($"Payment method '{request.PaymentMethod}' does not use hosted checkout — use /mobile-money instead");
                break;
        }

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }

    public void ValidatePayout(PayoutRequest request)
    {
        var errors = new List<string>();

        ValidateCommon(errors, request.Amount, request.Currency, request.MerchantReference);

        switch (request.PaymentMethod)
        {
            case PaymentMethod.MobileMoney:
                if (!MobileMoneyProviders.Contains(request.Provider))
                    errors.Add($"Provider '{request.Provider}' is not a supported Mobile Money provider");
                if (string.IsNullOrWhiteSpace(request.PhoneNumber) || !PhoneNumberRegex().IsMatch(request.PhoneNumber))
                    errors.Add("Phone number must be a valid MSISDN for a Mobile Money payout");
                break;
            case PaymentMethod.BankTransfer:
                if (string.IsNullOrWhiteSpace(request.BankAccountNumber))
                    errors.Add("Bank account number is required for bank transfer payouts");
                if (string.IsNullOrWhiteSpace(request.BankName))
                    errors.Add("Bank name is required for bank transfer payouts");
                break;
            default:
                errors.Add($"Payment method '{request.PaymentMethod}' is not supported for payouts");
                break;
        }

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }

    private void ValidateCommon(List<string> errors, decimal amount, string currency, string merchantReference)
    {
        if (amount <= 0)
            errors.Add("Amount must be greater than zero");

        if (string.IsNullOrWhiteSpace(currency) || !_gatewaySettings.SupportedCurrencies.Contains(currency, StringComparer.OrdinalIgnoreCase))
            errors.Add($"Currency '{currency}' is not supported. Supported currencies: {string.Join(", ", _gatewaySettings.SupportedCurrencies)}");

        if (string.IsNullOrWhiteSpace(merchantReference))
            errors.Add("Merchant reference is required");
    }

    private static bool IsValidCardNumber(string digitsOnly, PaymentProvider provider)
    {
        if (digitsOnly.Length is < 13 or > 19 || !IsLuhnValid(digitsOnly))
            return false;

        return provider switch
        {
            PaymentProvider.Visa => digitsOnly.StartsWith('4'),
            PaymentProvider.Mastercard => IsMastercardBin(digitsOnly),
            _ => false
        };
    }

    private static bool IsMastercardBin(string digits)
    {
        var prefix2 = int.Parse(digits[..2]);
        var prefix4 = int.Parse(digits[..4]);
        return prefix2 is >= 51 and <= 55 || prefix4 is >= 2221 and <= 2720;
    }

    private static bool IsLuhnValid(string digits)
    {
        var sum = 0;
        var alternate = false;
        for (var i = digits.Length - 1; i >= 0; i--)
        {
            var n = digits[i] - '0';
            if (alternate)
            {
                n *= 2;
                if (n > 9) n -= 9;
            }
            sum += n;
            alternate = !alternate;
        }
        return sum % 10 == 0;
    }

    [GeneratedRegex(@"^\+?\d{9,15}$")]
    private static partial Regex PhoneNumberRegex();

    [GeneratedRegex(@"\D")]
    private static partial Regex DigitsOnlyRegex();

    [GeneratedRegex(@"^\d{3,4}$")]
    private static partial Regex CvvRegex();
}
