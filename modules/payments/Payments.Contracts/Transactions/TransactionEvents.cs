namespace Payments.Contracts.Transactions
{
    public record PaymentProcessedEvent(Guid OrderId);
    public record PaymentFailedEvent(Guid OrderId, string Reason);
}
