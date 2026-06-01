using Inventory.Contracts.Stocks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ordering.Contracts.Orders;
using Payments.Api.Infrastructure;
using Payments.Api.Infrastructure.Payments;
using Payments.Contracts.Transactions;

namespace Payments.Api.Features.Transactions.Consumers
{
    public class OrderPaymentConsumer(
        PaymentsDBContext dbContext,
        ILogger<OrderPaymentConsumer> logger,
        IPaymentProcessor paymentProcessor
        ) : IConsumer<InventoryReservedEvent>
    {
        public async Task Consume(ConsumeContext<InventoryReservedEvent> context)
        {
            var message = context.Message;
            var ct = context.CancellationToken;

            var transaction = new Transaction(message.OrderId, message.Amount);

            // Check if a transaction for this order already exists to ensure idempotency
            var alreadyExists = await dbContext.Transactions.AnyAsync(t => t.OrderId == message.OrderId, ct);
            if (alreadyExists) return;

            bool isSuccess = paymentProcessor.Process(message.OrderId, message.Amount);
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

            try
            {
                await dbContext.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                // This error could happen if another consumer has already processed this order and inserted a transaction, violating the unique constraint on OrderId
                logger.LogError(ex, "An error occurred while saving the transaction for Order {OrderId}", message.OrderId);
            }
            logger.LogInformation("Transaction for Order {OrderId} processed. Success: {IsSuccess}", message.OrderId, transaction.IsSuccess);
        }
    }
}