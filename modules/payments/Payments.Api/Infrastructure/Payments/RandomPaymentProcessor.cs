namespace Payments.Api.Infrastructure.Payments
{
    public class RandomPaymentProcessor : IPaymentProcessor
    {
        public bool Process(Guid orderId, decimal amount)
            => Random.Shared.Next(1, 10) <= 9;
    }
}
