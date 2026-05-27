namespace Ordering.Contracts
{
    public record OrderCreated(Guid Id, Guid ProductId, int Quantity, decimal TotalPrice);
}
