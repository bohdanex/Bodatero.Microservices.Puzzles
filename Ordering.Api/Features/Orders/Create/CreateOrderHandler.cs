using MassTransit;
using MediatR;
using Ordering.Contracts;
using Ordering.Api.Infrastructure;
using System.Diagnostics;

namespace Ordering.Api.Features.Orders.Create
{
    public record CreateOrderCommand(Guid ProductId, int Quantity) : IRequest<Guid>;
    public class CreateOrderHandler(OrderingDBContext dbContext, IBus bus, ILogger<CreateOrderHandler> logger) : IRequestHandler<CreateOrderCommand, Guid>
    {
        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var orderId = Guid.NewGuid();

            decimal pricePerUnit = 100.00m;
            decimal totalPrice = pricePerUnit * request.Quantity;

            var order = new Order()
            {
                Id = orderId,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                Status = OrderStatus.Created,
                TotalPrice = totalPrice, // Suppose we sell the same item with unchanged price. In real scenarios, we should get the price from product service.
            };

            dbContext.Orders.Add(order);

            logger.LogInformation("Saving order to database with Id: {OrderId}", orderId);

            await bus.Publish(new OrderCreated(orderId, request.ProductId, request.Quantity, order.TotalPrice), cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Order created with Id: {OrderId}, ProductId: {ProductId}, Quantity: {Quantity}, TotalPrice: {TotalPrice}", orderId, request.ProductId, request.Quantity, totalPrice);

            return orderId;
        }
    }
}
