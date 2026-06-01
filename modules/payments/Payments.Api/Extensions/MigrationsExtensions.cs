using Microsoft.EntityFrameworkCore;
using Payments.Api.Infrastructure;

namespace Payments.Api.Extensions
{
    public static class MigrationsExtensions
    {
        public static WebApplication ApplyMigrations(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDBContext>();
            
            if (dbContext.Database.GetPendingMigrations().Any())
            {
                dbContext.Database.Migrate();
            }

            return app;
        }
    }
}
