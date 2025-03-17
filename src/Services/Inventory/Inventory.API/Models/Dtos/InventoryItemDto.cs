namespace Inventory.API.Models.Dtos;

public class InventoryItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; }
    public int QuantityAvailable { get; set; }
    public int QuantityReserved { get; set; }
    public int ReorderThreshold { get; set; }
    public int ReorderQuantity { get; set; }
    public DateTime LastRestockDate { get; set; }
    public string WarehouseCode { get; set; }

    // No circular reference
    public List<InventoryMovementDto> RecentMovements { get; set; } = new List<InventoryMovementDto>();
}