using System;
using System.Text.Json.Serialization;

namespace Inventory.API.Models;

public class InventoryMovement
{
    public Guid Id { get; set; }
    public Guid InventoryItemId { get; set; }
    public int Quantity { get; set; }
    public InventoryMovementType Type { get; set; }
    public Guid? ReferenceId { get; set; }
    public string ReferenceType { get; set; }
    public DateTime Timestamp { get; set; }
    public string InitiatedBy { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual InventoryItem InventoryItem { get; set; }
}

public enum InventoryMovementType
{
    StockIn,
    StockOut,
    Reservation,
    ReservationRelease,
    Adjustment

}