namespace PaymentMock.Enums;

public enum TransactionStatus
{
    Created,
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled,
    Expired,
    AwaitingCheckout
}
