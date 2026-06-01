using MassTransit;
using Microsoft.EntityFrameworkCore;
using Payments.Api.Features.Transactions;

namespace Payments.Api.Infrastructure
{
    public class PaymentsDBContext : DbContext
    {
        public DbSet<Transaction> Transactions { get; set; }

        public PaymentsDBContext(DbContextOptions<PaymentsDBContext> options)
            : base(options)
        {

        }

        override protected void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddTransactionalOutboxEntities();

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.OrderId).IsUnique(); // Ensure one transaction per order
            });
        }
    }
}
