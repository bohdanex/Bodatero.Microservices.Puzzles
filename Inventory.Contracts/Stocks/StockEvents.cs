namespace Inventory.Contracts.Stocks
{
    public record InventoryReservedEvent(Guid OrderId, decimal Amount);
    public record InventoryReservationFailedEvent(Guid OrderId, Guid ProductId, string Reason);
}
