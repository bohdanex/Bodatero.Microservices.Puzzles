namespace Inventory.Api.Features.Stocks
{
    public class Stock
    {
        public Guid ProductId { get; private set; }
        public int AvailableQuantity { get; private set; }

        private Stock() { }

        public void Reserve(int quantity)
        {
            if (AvailableQuantity < quantity)
                throw new Exception("Low stock!");

            AvailableQuantity -= quantity;
        }
    }
}
