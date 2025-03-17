using System;
using System.Collections.Generic;

namespace Inventory.API.Models;

public class InventoryItem
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
    public virtual ICollection<InventoryMovement> Movements { get; set; } = new List<InventoryMovement>();
}