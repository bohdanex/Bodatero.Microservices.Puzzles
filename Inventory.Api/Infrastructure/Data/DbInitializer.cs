using Inventory.Api.Features.Stocks;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedDevDataAsync(InventoryDBContext dbContext)
    {
        // Check if there are any stocks already in the database to avoid seeding duplicate data
        if (await dbContext.Stocks.AnyAsync())
            return;

        var initialStocks = new List<Stock>
        {
            CreateStock(Guid.Parse("6f3a38a9-dc22-4876-bc3d-bf66fbc26bc1"), 100),
            CreateStock(Guid.Parse("8a4b39b0-ed33-4987-cd4e-cf77fcd37cd2"), 50)
        };

        await dbContext.Stocks.AddRangeAsync(initialStocks);
        await dbContext.SaveChangesAsync();
    }

    private static Stock CreateStock(Guid productId, int quantity)
    {
        var stock = (Stock)Activator.CreateInstance(typeof(Stock), true)!;

        typeof(Stock).GetProperty(nameof(Stock.ProductId))?
            .SetValue(stock, productId);

        typeof(Stock).GetProperty(nameof(Stock.AvailableQuantity))?
            .SetValue(stock, quantity);

        return stock;
    }
}