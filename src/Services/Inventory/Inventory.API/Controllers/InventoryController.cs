using Inventory.API.Models;
using Inventory.API.Models.Dtos;
using Inventory.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
    {
        _inventoryService = inventoryService;
        _logger = logger;
    }

    [HttpGet("{productId}")]
    public async Task<ActionResult<InventoryItemDto>> GetByProductId(Guid productId)
    {
        var item = await _inventoryService.GetByProductIdAsync(productId);
        if (item == null)
        {
            return NotFound();
        }

        // Separately get the movements to avoid circular references
        var movements = await _inventoryService.GetMovementsForProductAsync(productId, 10);

        // Map to DTO to avoid circular references
        var dto = new InventoryItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            Sku = item.Sku,
            QuantityAvailable = item.QuantityAvailable,
            QuantityReserved = item.QuantityReserved,
            ReorderThreshold = item.ReorderThreshold,
            ReorderQuantity = item.ReorderQuantity,
            LastRestockDate = item.LastRestockDate,
            WarehouseCode = item.WarehouseCode,
            RecentMovements = movements.Select(m => new InventoryMovementDto
            {
                Id = m.Id,
                Quantity = m.Quantity,
                Type = m.Type,
                ReferenceId = m.ReferenceId,
                ReferenceType = m.ReferenceType,
                Timestamp = m.Timestamp,
                InitiatedBy = m.InitiatedBy
            }).ToList()
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<InventoryItem>> CreateInventoryItem(InventoryItem item)
    {
        var result = await _inventoryService.CreateInventoryItemAsync(item);
        return CreatedAtAction(nameof(GetByProductId), new { productId = result.ProductId }, result);
    }

    [HttpPost("reserve")]
    public async Task<IActionResult> ReserveStock([FromBody] ReserveStockRequest request)
    {
        var result = await _inventoryService.ReserveStockAsync(request.ProductId, request.Quantity, request.OrderId);

        if (result) return Ok();

        return BadRequest("Failed to reserve stock. Check if sufficient stock is available.");
    }

    [HttpPost("release")]
    public async Task<IActionResult> ReleaseStock([FromBody] ReleaseStockRequest request)
    {
        var result = await _inventoryService.ReleaseStockAsync(request.ProductId, request.Quantity, request.OrderId);

        if (result) return Ok();

        return BadRequest("Failed to release stock. Check if the stock is reserved for the order.");
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> AdjustStock([FromBody] AdjustStockRequest request)
    {
        var result = await _inventoryService.AdjustStockAsync(request.ProductId, request.NewQuantity, request.Reason);

        if (result) return Ok();

        return BadRequest("Failed to adjust stock. Check if the new quantity is valid.");
    }
}

public class ReserveStockRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public Guid OrderId { get; set; }
}

public class ReleaseStockRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public Guid OrderId { get; set; }
}

public class AdjustStockRequest
{
    public Guid ProductId { get; set; }
    public int NewQuantity { get; set; }
    public string Reason { get; set; }
}