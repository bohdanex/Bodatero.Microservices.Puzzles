using Inventory.Api.Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace Inventory.IntegrationTests.Infrastructure
{
    public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:latest")
            .WithDatabase("bodatero_ecommerce_inventory")
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
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<InventoryDBContext>));

                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<InventoryDBContext>(options =>
                {
                    options
                        .UseNpgsql(_dbContainer.GetConnectionString());
                });
            });

            builder.ConfigureTestServices(services =>
            {
                services.AddMassTransitTestHarness();
            });
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
