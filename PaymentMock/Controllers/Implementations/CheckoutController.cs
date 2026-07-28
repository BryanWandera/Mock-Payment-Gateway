using System.Net;
using Microsoft.AspNetCore.Mvc;
using PaymentMock.Controllers.Interfaces;
using PaymentMock.DTOs.Generic;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Controllers.Implementations;

/// <summary>
/// Serves the hosted checkout page opened by a browser/WebView for Card and Bank Transfer
/// payments — the piece that was previously missing entirely from this mock gateway (see
/// HostedCheckoutRequest/InitiateHostedCheckoutAsync in PaymentService). Not under api/v1 and
/// not JSON: this is a bare HTML page with no auth headers, so it's exempted in both
/// ApiKeyAuthenticationMiddleware and PesapalBearerAuthenticationMiddleware.
/// </summary>
[Route("checkout")]
public class CheckoutController : ControllerBase, ICheckoutController
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> GetCheckoutPage(string token)
    {
        var result = await _checkoutService.GetCheckoutPageAsync(token);
        return Content(CheckoutPageHtml.RenderActionPage(result), "text/html");
    }

    [HttpPost("{token}/approve")]
    public async Task<IActionResult> Approve(string token)
    {
        await _checkoutService.ApproveAsync(token);
        return Redirect($"/checkout/{Uri.EscapeDataString(token)}/complete");
    }

    [HttpPost("{token}/decline")]
    public async Task<IActionResult> Decline(string token)
    {
        await _checkoutService.DeclineAsync(token);
        return Redirect($"/checkout/{Uri.EscapeDataString(token)}/complete");
    }

    [HttpGet("{token}/complete")]
    public async Task<IActionResult> GetCompletePage(string token)
    {
        var result = await _checkoutService.GetCheckoutPageAsync(token);
        return Content(CheckoutPageHtml.RenderCompletePage(result), "text/html");
    }
}

/// <summary>
/// Minimal inline HTML templates for the mock checkout flow. There's no Razor/static-file
/// pipeline in this project (see Program.cs), so this stays a small presentation-only helper
/// rather than pulling in view infrastructure for two pages.
/// </summary>
internal static class CheckoutPageHtml
{
    private const string Style = """
        <style>
            body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                   background: #F2F7C9; margin: 0; padding: 0; display: flex; min-height: 100vh;
                   align-items: center; justify-content: center; }
            .card { background: #FFFFFF; border-radius: 16px; padding: 32px 24px; max-width: 400px;
                    width: 90%; box-shadow: 0 4px 24px rgba(0,0,0,0.12); }
            .badge { display: inline-block; background: #022126; color: #D8E945; font-size: 12px;
                     font-weight: 600; padding: 4px 10px; border-radius: 12px; margin-bottom: 16px; }
            h1 { color: #022126; font-size: 20px; margin: 0 0 4px; }
            .amount { color: #022126; font-size: 32px; font-weight: 700; margin: 8px 0; }
            .row { display: flex; justify-content: space-between; padding: 8px 0;
                   border-bottom: 1px solid #E5E7EB; font-size: 14px; color: #6B7280; }
            .row span:last-child { color: #022126; font-weight: 500; }
            .actions { margin-top: 24px; }
            button { width: 100%; padding: 14px; border: none; border-radius: 8px; font-size: 16px;
                     font-weight: 600; cursor: pointer; margin-bottom: 12px; }
            .approve { background: #D8E945; color: #022126; }
            .decline { background: #F5F5F5; color: #6B7280; }
            .status { text-align: center; }
            .status-icon { font-size: 48px; margin-bottom: 8px; }
            .status.completed .status-icon { color: #10B981; }
            .status.failed .status-icon { color: #EF4444; }
            .status.pending .status-icon { color: #F59E0B; }
            p.hint { color: #6B7280; font-size: 13px; text-align: center; margin-top: 16px; }
        </style>
        """;

    public static string RenderActionPage(CheckoutPageResult result)
    {
        if (!result.Found)
            return Wrap("<div class=\"status\"><div class=\"status-icon\">&#10060;</div><h1>Checkout link not found</h1>" +
                         "<p class=\"hint\">This checkout link is invalid or has expired.</p></div>");

        if (!result.IsAwaitingAction)
            return RenderCompletePage(result);

        var token = Encode(result.CheckoutToken);
        return Wrap($"""
            <span class="badge">MOCK PAYMENT GATEWAY</span>
            <h1>Confirm your payment</h1>
            <div class="amount">{Encode(result.Currency)} {result.Amount:N2}</div>
            <div class="row"><span>Merchant reference</span><span>{Encode(result.MerchantReference)}</span></div>
            <div class="row"><span>Payment method</span><span>{Encode(FormatMethod(result.PaymentMethod))}</span></div>
            {(string.IsNullOrWhiteSpace(result.CustomerName) ? "" : $"<div class=\"row\"><span>Customer</span><span>{Encode(result.CustomerName)}</span></div>")}
            {(result.PaymentMethod == "BankTransfer" ? BankDetailsBlock(result) : "")}
            <div class="actions">
                <form method="post" action="/checkout/{token}/approve">
                    <button type="submit" class="approve">Approve payment</button>
                </form>
                <form method="post" action="/checkout/{token}/decline">
                    <button type="submit" class="decline">Cancel</button>
                </form>
            </div>
            <p class="hint">This is a mock checkout page for testing — no real money moves.</p>
            """);
    }

    public static string RenderCompletePage(CheckoutPageResult result)
    {
        if (!result.Found)
            return Wrap("<div class=\"status\"><div class=\"status-icon\">&#10060;</div><h1>Checkout link not found</h1></div>");

        var (icon, cssClass, title, hint) = result.Status switch
        {
            "Completed" => ("&#9989;", "completed", "Payment approved", "You can close this window and return to the app."),
            "Failed" => ("&#10060;", "failed", "Payment failed", "You can close this window and return to the app."),
            "AwaitingCheckout" => ("&#9203;", "pending", "Awaiting your confirmation", "Go back to confirm or cancel this payment."),
            _ => ("&#9203;", "pending", "Processing your payment…", "This can take a few seconds. You can close this window and return to the app.")
        };

        return Wrap($"""
            <div class="status {cssClass}">
                <div class="status-icon">{icon}</div>
                <h1>{title}</h1>
                <div class="amount">{Encode(result.Currency)} {result.Amount:N2}</div>
                <div class="row"><span>Merchant reference</span><span>{Encode(result.MerchantReference)}</span></div>
                <div class="row"><span>Status</span><span>{Encode(result.Status)}</span></div>
                <p class="hint">{hint}</p>
            </div>
            """);
    }

    private static string BankDetailsBlock(CheckoutPageResult result) => $"""
        <div class="row"><span>Bank</span><span>Mock Test Bank</span></div>
        <div class="row"><span>Account number</span><span>0100{result.TransactionId[..6].ToUpperInvariant()}</span></div>
        """;

    private static string FormatMethod(string method) => method switch
    {
        "BankTransfer" => "Bank Transfer",
        var other => other
    };

    private static string Wrap(string bodyHtml) => $"""
        <!doctype html>
        <html lang="en">
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1" />
            <title>Mock Payment Gateway Checkout</title>
            {Style}
        </head>
        <body>
            <div class="card">
                {bodyHtml}
            </div>
        </body>
        </html>
        """;

    private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
}
