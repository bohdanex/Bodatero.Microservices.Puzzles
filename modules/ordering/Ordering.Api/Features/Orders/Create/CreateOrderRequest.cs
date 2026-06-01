namespace Ordering.Api.Features.Orders.Create
{
    public record CreateOrderRequest(Guid ProductId, int Quantity);
}
