using MassTransit.Testing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Api.Infrastructure;

namespace Ordering.IntegrationTests.Infrastructure
{
    public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
    {
        private readonly IServiceScope _serviceScope;
        protected readonly ISender Sender;
        protected readonly OrderingDBContext DbContext;
        protected readonly ITestHarness TestHarness;
        protected readonly HttpClient HttpClient;

        protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
        {
            _serviceScope = factory.Services.CreateScope();
            Sender = _serviceScope.ServiceProvider.GetRequiredService<ISender>();
            DbContext = _serviceScope.ServiceProvider.GetRequiredService<OrderingDBContext>();
            TestHarness = _serviceScope.ServiceProvider.GetRequiredService<ITestHarness>();
            HttpClient = factory.CreateClient();
        }

        public void Dispose()
        {
            _serviceScope?.Dispose();
            HttpClient?.Dispose();
        }
    }
}
