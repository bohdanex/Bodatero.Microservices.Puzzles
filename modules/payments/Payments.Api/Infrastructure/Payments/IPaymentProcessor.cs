namespace Payments.Api.Infrastructure.Payments
{
    public interface IPaymentProcessor
    {
        bool Process(Guid orderId, decimal amount);
    }
}
