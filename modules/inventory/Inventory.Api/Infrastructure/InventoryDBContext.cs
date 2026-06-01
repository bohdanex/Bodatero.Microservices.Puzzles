using Inventory.Api.Features.Stocks;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Infrastructure
{
    public class InventoryDBContext : DbContext
    {
        public DbSet<Stock> Stocks { get; set; }

        public InventoryDBContext(DbContextOptions<InventoryDBContext> options)
            : base(options)
        {

        }

        override protected void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddTransactionalOutboxEntities();

            modelBuilder.Entity<Stock>()
                .HasKey(s => s.ProductId);
        }
    }
}
