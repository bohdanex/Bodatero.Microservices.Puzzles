namespace Inventory.Contracts.Stocks
{
    public record InventoryReservedEvent(Guid OrderId, decimal Amount);
}
