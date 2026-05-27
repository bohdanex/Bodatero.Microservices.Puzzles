using Microsoft.EntityFrameworkCore;
using Ordering.Api.Infrastructure;

namespace Ordering.Api.Extensions
{
    public static class MigrationsExtensions
    {
        public static WebApplication ApplyMigrations(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDBContext>();
            
            if (dbContext.Database.GetPendingMigrations().Any())
            {
                dbContext.Database.Migrate();
            }

            return app;
        }
    }
}
