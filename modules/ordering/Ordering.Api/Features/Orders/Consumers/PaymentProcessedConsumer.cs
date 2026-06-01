using MassTransit;
using Ordering.Api.Infrastructure;
using Payments.Contracts.Transactions;

namespace Ordering.Api.Features.Orders.Consumers
{
    public class PaymentProcessedConsumer(OrderingDBContext dbContext, ILogger<PaymentProcessedConsumer> logger) : IConsumer<PaymentProcessedEvent>
    {
        public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
        {
            var ct = context.CancellationToken;
            var order = await dbContext.Orders.FindAsync(context.Message.OrderId, ct);

            if (order != null)
            {
                order.Complete();
                await dbContext.SaveChangesAsync(ct);
                logger.LogInformation("Order {OrderId} marked as completed", context.Message.OrderId);
            }
        }
    }
}
