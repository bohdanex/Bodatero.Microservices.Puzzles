using MassTransit;
using MediatR;
using Ordering.Contracts.Orders;
using Ordering.Api.Infrastructure;

namespace Ordering.Api.Features.Orders.Create
{
    public record CreateOrderCommand(Guid ProductId, int Quantity) : IRequest<Guid>;
    public class CreateOrderHandler(OrderingDBContext dbContext, IBus bus, ILogger<CreateOrderHandler> logger) : IRequestHandler<CreateOrderCommand, Guid>
    {
        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            decimal pricePerUnit = 100.00m;
            decimal totalPrice = pricePerUnit * request.Quantity;

            var order = new Order(request.ProductId, request.Quantity, totalPrice);
            var orderId = order.Id;

            dbContext.Orders.Add(order);

            await bus.Publish(new OrderCreatedEvent(orderId, request.ProductId, request.Quantity, order.TotalPrice), cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Order created with Id: {OrderId}, ProductId: {ProductId}, Quantity: {Quantity}, TotalPrice: {TotalPrice}", orderId, request.ProductId, request.Quantity, totalPrice);

            return orderId;
        }
    }
}
