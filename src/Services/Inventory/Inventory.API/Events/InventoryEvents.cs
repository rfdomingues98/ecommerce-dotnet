using System;

namespace Inventory.API.Events;

public abstract class InventoryEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; }
}

public class InventoryChangedEvent : InventoryEvent
{
    public InventoryChangedEvent()
    {
        EventType = "InventoryChanged";
    }

    public Guid ProductId { get; set; }
    public string Sku { get; set; }
    public int NewQuantityAvailable { get; set; }
    public int NewQuantityReserved { get; set; }
    public string ChangeReason { get; set; }
}

public class InventoryReservedEvent : InventoryEvent
{
    public InventoryReservedEvent()
    {
        EventType = "InventoryReserved";
    }

    public Guid ProductId { get; set; }
    public string Sku { get; set; }
    public int QuantityReserved { get; set; }
    public Guid OrderId { get; set; }
}

public class LowStockEvent : InventoryEvent
{
    public LowStockEvent()
    {
        EventType = "LowStock";
    }

    public Guid ProductId { get; set; }
    public string Sku { get; set; }
    public int CurrentStock { get; set; }
    public int ReorderThreshold { get; set; }
    public int RecommendedReorderQuantity { get; set; }
}
