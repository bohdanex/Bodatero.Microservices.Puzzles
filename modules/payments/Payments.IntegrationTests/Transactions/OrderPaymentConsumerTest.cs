using FluentAssertions;
using Inventory.Contracts.Stocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using Ordering.Contracts.Orders;
using Payments.Api.Infrastructure.Payments;
using Payments.Contracts.Transactions;
using Payments.IntegrationTests.Infrastructure;

namespace Payments.IntegrationTests.Transactions
{
    public class OrderPaymentConsumerTest : BaseIntegrationTest
    {
        private readonly IPaymentProcessor _paymentProcessorMock;

        public OrderPaymentConsumerTest(IntegrationTestWebAppFactory factory) : base(factory)
        {
            _paymentProcessorMock = factory.PaymentProcessor;
            _paymentProcessorMock.ClearSubstitute();

            DbContext.Transactions.RemoveRange(DbContext.Transactions);
            DbContext.SaveChanges();
        }

        [Fact]
        public async Task ShouldPublishOrderFailedEvent_WhenTransactionNotSucceeded()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var inventoryReservedEvent = new InventoryReservedEvent(orderId, 100m);
            _paymentProcessorMock.Process(Arg.Any<Guid>(), Arg.Any<decimal>()).Returns(false);

            // Act
            await TestHarness.Bus.Publish(inventoryReservedEvent);

            // Assert
            var consumed = await TestHarness.Consumed.Any<InventoryReservedEvent>(x => x.Message?.OrderId == orderId);
            var transaction = await DbContext.Transactions.FirstAsync();
            consumed.Should().BeTrue();
            transaction.IsSuccess.Should().BeFalse();
            var orederFailedEventPublished = await TestHarness.Published.Any<OrderFailedEvent>(x => x.Message?.OrderId == orderId);
            orederFailedEventPublished.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldPublishPaymentProcessedEvent_WhenTransactionSucceeded()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var inventoryReservedEvent = new InventoryReservedEvent(orderId, 100m);
            _paymentProcessorMock.Process(Arg.Any<Guid>(), Arg.Any<decimal>()).Returns(true);

            // Act
            await TestHarness.Bus.Publish(inventoryReservedEvent);

            // Assert
            var consumed = await TestHarness.Consumed.Any<InventoryReservedEvent>(x => x.Message?.OrderId == orderId);
            var transaction = await DbContext.Transactions.FirstAsync();
            consumed.Should().BeTrue();
            transaction.IsSuccess.Should().BeTrue();
            var orederFailedEventPublished = await TestHarness.Published.Any<PaymentProcessedEvent>(x => x.Message?.OrderId == orderId);
            orederFailedEventPublished.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldHandleIdempotency_WhenMessageRepeated()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _paymentProcessorMock.Process(Arg.Any<Guid>(), Arg.Any<decimal>()).Returns(true);

            // Act
            await Task.WhenAll(TestHarness.Bus.Publish(new InventoryReservedEvent(orderId, 100m)), TestHarness.Bus.Publish(new InventoryReservedEvent(orderId, 100m)));

            // Assert
            var consumed = await TestHarness.Consumed.Any<InventoryReservedEvent>(x => x.Message?.OrderId == orderId);
            var transactions = await DbContext.Transactions.Where(x => x.OrderId == orderId).ToListAsync();
            consumed.Should().BeTrue();
            transactions.Should().HaveCount(1);
        }
    }
}
