using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ordering.Api.Features.Orders;

namespace Ordering.Api.Infrastructure
{
    public class OrderingDBContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }

        public OrderingDBContext(DbContextOptions<OrderingDBContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("ordering");
            modelBuilder.AddTransactionalOutboxEntities();

            modelBuilder.Entity<Order>(o =>
            {
                o.HasKey(x => x.Id);
            });
        }
    }
}
