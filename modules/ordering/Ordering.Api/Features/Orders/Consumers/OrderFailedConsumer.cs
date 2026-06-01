using MassTransit;
using Ordering.Api.Infrastructure;
using Ordering.Contracts.Orders;

namespace Ordering.Api.Features.Orders.Consumers
{
    public class OrderFailedConsumer(OrderingDBContext dbContext, ILogger<OrderFailedConsumer> logger) : IConsumer<OrderFailedEvent>
    {
        public async Task Consume(ConsumeContext<OrderFailedEvent> context)
        {
            var ct = context.CancellationToken;
            var order = await dbContext.Orders.FindAsync(context.Message.OrderId, ct);

            if (order != null)
            {
                order.Fail();
                await dbContext.SaveChangesAsync(ct);
                logger.LogInformation("Order {OrderId} marked as failed. Reason: {Reason}", context.Message.OrderId, context.Message.Reason);
            }
        }
    }
}
