namespace Payments.Api.Features.Transactions
{
    public class Transaction
    {
        public Guid Id { get; private set; }
        public Guid OrderId { get; private set; }
        public decimal Amount { get; private set; }
        public bool IsSuccess { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Transaction() { }

        public Transaction(Guid orderId, decimal amount)
        {
            Id = Guid.NewGuid();
            OrderId = orderId;
            Amount = amount;
            CreatedAt = DateTime.UtcNow;
        }

        public void ProcessPayment(bool simulationSuccess)
        {
            IsSuccess = simulationSuccess;
        }
    }
}
