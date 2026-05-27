namespace Ordering.Api.Features.Orders
{
    public class Order
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        private Order() { }

        public Order(Guid productId, int quantity, decimal totalPrice)
        {
            Id = Guid.NewGuid();
            ProductId = productId;
            Quantity = quantity;
            TotalPrice = totalPrice;
            Status = OrderStatus.Pending;
        }

        public void Complete()
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOperationException("Can only complete pending orders.");

            Status = OrderStatus.Completed;
        }

        public void Fail()
        {
            Status = OrderStatus.Failed;
        }
    }
}
