using Inventory.Contracts.Stocks;
using MassTransit;
using Ordering.Contracts.Orders;
using Payments.Api.Infrastructure;
using Payments.Contracts.Transactions;

namespace Payments.Api.Features.Transactions.Consumers
{
    public class InventoryReservedConsumer(PaymentsDBContext dbContext, ILogger<InventoryReservedConsumer> logger) : IConsumer<InventoryReservedEvent>
    {
        public async Task Consume(ConsumeContext<InventoryReservedEvent> context)
        {
            var message = context.Message;
            var ct = context.CancellationToken;

            var transaction = new Transaction(message.OrderId, message.Amount);

            // Imitating 90% success rate for payments
            bool isSuccess = Random.Shared.Next(1, 10) <= 9;
            transaction.ProcessPayment(isSuccess);

            dbContext.Transactions.Add(transaction);

            if (transaction.IsSuccess)
            {
                await context.Publish(new PaymentProcessedEvent(message.OrderId), ct);
            }
            else
            {
                await context.Publish(new OrderFailedEvent(message.OrderId, "Payment declined"), ct);
            }

            await dbContext.SaveChangesAsync(ct);
            logger.LogInformation("Transaction for Order {OrderId} processed. Success: {IsSuccess}", message.OrderId, transaction.IsSuccess);
        }
    }
}