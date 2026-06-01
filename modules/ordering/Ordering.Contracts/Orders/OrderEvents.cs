namespace Ordering.Contracts.Orders
{
    public record OrderCreatedEvent(Guid Id, Guid ProductId, int Quantity, decimal TotalPrice);
    public record OrderFailedEvent(Guid OrderId, string Reason);
}
