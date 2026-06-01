using FluentAssertions;
using Inventory.Api.Features.Stocks;
using Inventory.Contracts.Stocks;
using Microsoft.EntityFrameworkCore;
using Ordering.Contracts.Orders;
using Inventory.IntegrationTests.Infrastructure;

namespace Inventory.IntegrationTests.Stocks
{
    public class OrderCreatedTest : BaseIntegrationTest
    {
        public OrderCreatedTest(IntegrationTestWebAppFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task ShouldPublishOrderFailedEvent_WhenItemNotFound()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var orderCreatedEvent = new OrderCreatedEvent(orderId, productId, 1, 100);

            // Act
            await TestHarness.Bus.Publish(orderCreatedEvent);
            var consumed = await TestHarness.Consumed.Any<OrderCreatedEvent>(e => e.Message?.ProductId == productId && e.Message?.Id == orderId);

            // Assert
            consumed.Should().BeTrue();
            var hasOrderFailedEvent = await TestHarness.Published.Any<OrderFailedEvent>(e => e.Message?.OrderId == orderId);
            hasOrderFailedEvent.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldPublishOrderFailedEvent_WhenItemQuantityIsInsufficient()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var stock = Stock.CreateNew(productId, 2);
            
            DbContext.Stocks.Add(stock);
            await DbContext.SaveChangesAsync();

            // Act
            var orderCreatedEvent = new OrderCreatedEvent(orderId, productId, 4, 100);
            await TestHarness.Bus.Publish(orderCreatedEvent);

            // Assert
            var consumed = await TestHarness.Consumed.Any<OrderCreatedEvent>(e => e.Message?.ProductId == productId && e.Message?.Id == orderId);
            consumed.Should().BeTrue();
            var hasOrderFailedEvent = await TestHarness.Published.Any<OrderFailedEvent>(e => e.Message?.OrderId == orderId);
            hasOrderFailedEvent.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldUpdateItemQuantity_ThenPublishInventoryReservedEvent()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var stock = Stock.CreateNew(productId, 4);
            
            DbContext.Stocks.Add(stock);
            await DbContext.SaveChangesAsync();

            // Act
            var orderCreatedEvent = new OrderCreatedEvent(orderId, productId, 4, 100);
            await TestHarness.Bus.Publish(orderCreatedEvent);

            // Assert
            var consumed = await TestHarness.Consumed.Any<OrderCreatedEvent>(e => e.Message?.ProductId == productId && e.Message?.Id == orderId);
            consumed.Should().BeTrue();
            var hasInventoryReservedEvent = await TestHarness.Published.Any<InventoryReservedEvent>(e => e.Message?.OrderId == orderId);
            var updatedStock = await DbContext.Stocks.AsNoTracking().FirstOrDefaultAsync(x => x.ProductId == productId);
            updatedStock.Should().NotBeNull();
            updatedStock.AvailableQuantity.Should().Be(0);
            hasInventoryReservedEvent.Should().BeTrue();
        }
    }
}
