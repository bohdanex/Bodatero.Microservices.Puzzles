using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Ordering.Api.Features;
using Ordering.Api.Features.Orders.Create;
using Ordering.Contracts.Orders;
using Ordering.IntegrationTests.Infrastructure;
using Payments.Contracts.Transactions;
using System.Net.Http.Json;

namespace Ordering.IntegrationTests.Orders
{
    public class CreateOrderTest : BaseIntegrationTest
    {
        public CreateOrderTest(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task ShouldReturn404Error_WhenInvalidGuid()
        {
            // Arrange
            var request = new CreateOrderRequest(Guid.Empty, 0);
            // Act
            var response = await HttpClient.PostAsJsonAsync("/orders", request);
            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ShouldReturn404Error_WhenInvalidQuantity()
        {
            // Arrange
            var request = new CreateOrderRequest(Guid.NewGuid(), -1);
            // Act
            var response = await HttpClient.PostAsJsonAsync("/orders", request);
            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ShouldPublishEventAndSaveOrder_WhenValidRequest()
        {
            // Arrange
            var request = new CreateOrderRequest(Guid.NewGuid(), 1);

            // Act
            var response = await HttpClient.PostAsJsonAsync("/orders", request);

            // Assert
            var orderId = await response.Content.ReadFromJsonAsync<Guid>();
            var order = await DbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            var orderCreatedMessagePublished = await TestHarness.Published.Any<OrderCreatedEvent>();
            orderCreatedMessagePublished.Should().BeTrue();
            order.Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldFailOrder_WhenOrderFails()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var request = new CreateOrderRequest(productId, 1);

            // Act
            var response = await HttpClient.PostAsJsonAsync("/orders", request);
            var orderId = await response.Content.ReadFromJsonAsync<Guid>();
            var orderFailedEvent = new OrderFailedEvent(orderId, "Product not found");
            await TestHarness.Bus.Publish(orderFailedEvent);

            // Assert
            var consumed = await TestHarness.Consumed.Any<OrderFailedEvent>(x => x.Message?.OrderId == orderId);
            var order = await DbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            order.Should().NotBeNull();
            if (order != null)
            {
                order.Status.Should().Be(OrderStatus.Failed);
            }
        }

        [Fact]
        public async Task ShouldCompleteOrder_WhenPaymentProcessed()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var request = new CreateOrderRequest(productId, 1);

            // Act
            var response = await HttpClient.PostAsJsonAsync("/orders", request);
            var orderId = await response.Content.ReadFromJsonAsync<Guid>();
            var paymentProcessedEvent = new PaymentProcessedEvent(orderId);
            await TestHarness.Bus.Publish(paymentProcessedEvent);

            // Assert
            var consumed = await TestHarness.Consumed.Any<PaymentProcessedEvent>(x => x.Message?.OrderId == orderId);
            var order = await DbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            Assert.NotNull(order);
            if (order != null)
            {
                order.Status.Should().Be(OrderStatus.Completed);
            }
        }
    }
}
