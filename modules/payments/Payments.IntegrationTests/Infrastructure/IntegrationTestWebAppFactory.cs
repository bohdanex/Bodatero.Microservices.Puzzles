using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Payments.Api.Infrastructure;
using Payments.Api.Infrastructure.Payments;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace Payments.IntegrationTests.Infrastructure
{
    public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public readonly IPaymentProcessor PaymentProcessor = Substitute.For<IPaymentProcessor>();

        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:latest")
            .WithDatabase("bodatero_ecommerce_payments")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder("rabbitmq:latest")
            .WithUsername("guest")
            .WithPassword("guest")
            .Build();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "RabbitMq:Host", _rabbitMqContainer.Hostname },
                    { "RabbitMq:Port", _rabbitMqContainer.GetMappedPublicPort(5672).ToString() },
                    { "RabbitMq:Username", "guest" },
                    { "RabbitMq:Password", "guest" },
                    { "Data:Seed", "false" }
                });
            });

            builder.ConfigureTestServices(services =>
            {
                FindAndRemoveServiceDescriptor<DbContextOptions<PaymentsDBContext>>(services);

                services.AddDbContext<PaymentsDBContext>(options => options
                        .UseNpgsql(_dbContainer.GetConnectionString()));

                FindAndRemoveServiceDescriptor<IPaymentProcessor>(services);

                services.AddScoped(_ => PaymentProcessor);
            });

            builder.ConfigureTestServices(services =>
            {
                services.AddMassTransitTestHarness();
            });
        }

        private static void FindAndRemoveServiceDescriptor<T>(IServiceCollection services)
        {
            var serviceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
            if (serviceDescriptor != null) services.Remove(serviceDescriptor);
        }

        public Task InitializeAsync()
        {
            return Task.WhenAll(_dbContainer.StartAsync(), _rabbitMqContainer.StartAsync());
        }

        Task IAsyncLifetime.DisposeAsync()
        {
            return Task.WhenAll(_dbContainer.StopAsync(), _rabbitMqContainer.StopAsync());
        }
    }
}
