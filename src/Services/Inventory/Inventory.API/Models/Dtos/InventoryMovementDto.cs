namespace Inventory.API.Models.Dtos;

public class InventoryMovementDto
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
    public InventoryMovementType Type { get; set; }
    public Guid? ReferenceId { get; set; }
    public string ReferenceType { get; set; }
    public DateTime Timestamp { get; set; }
    public string InitiatedBy { get; set; }
}