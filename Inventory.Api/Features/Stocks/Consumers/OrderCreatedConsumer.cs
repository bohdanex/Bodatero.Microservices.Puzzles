using Inventory.Api.Infrastructure;
using Inventory.Contracts.Stocks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ordering.Contracts.Orders;

namespace Inventory.Api.Features.Stocks.Consumers
{
    public class OrderCreatedConsumer(InventoryDBContext dbContext, ILogger<OrderCreatedConsumer> logger) : IConsumer<OrderCreatedEvent>
    {
        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var message = context.Message;
            var ct = context.CancellationToken;

            var stock = await dbContext.Stocks
                .FirstOrDefaultAsync(s => s.ProductId == message.ProductId, ct);

            if (stock == null)
            {
                await context.Publish(new OrderFailedEvent(message.Id, "Product not found"));
                await dbContext.SaveChangesAsync(ct);
                logger.LogInformation("Stock not found for ProductId: {ProductId}", message.ProductId);
                return;
            }

            try
            {
                stock.Reserve(message.Quantity);

                await context.Publish(new InventoryReservedEvent(message.Id, message.TotalPrice));
                logger.LogInformation("Reserved {Quantity} units for ProductId: {ProductId}", message.Quantity, message.ProductId);
            }
            catch (Exception ex)
            {
                await context.Publish(new OrderFailedEvent(message.Id, ex.Message));
                logger.LogError(ex, "Error reserving stock for ProductId: {ProductId}", message.ProductId);
            }

            await dbContext.SaveChangesAsync(ct);
        }
    }
}