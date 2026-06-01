namespace Inventory.Api.Features.Stocks
{
    public class Stock
    {
        public Guid ProductId { get; private set; }
        public int AvailableQuantity { get; private set; }

        private Stock() { }
        public static Stock CreateNew(Guid productId, int quantity)
        {
            return new Stock
            {
                ProductId = productId,
                AvailableQuantity = quantity
            };
        }

        public void Reserve(int quantity)
        {
            if (AvailableQuantity < quantity)
                throw new Exception("Low stock!");

            AvailableQuantity -= quantity;
        }
    }
}
