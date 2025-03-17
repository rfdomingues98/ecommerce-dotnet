using Inventory.API.Data;
using Inventory.API.Events;
using Inventory.API.Models;
using Inventory.API.Services.Caching;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Services;

public interface IInventoryService
{
    Task<InventoryItem> GetByProductIdAsync(Guid productId);
    Task<bool> ReserveStockAsync(Guid productId, int quantity, Guid orderId);
    Task<bool> ReleaseStockAsync(Guid productId, int quantity, Guid orderId);
    Task<bool> AdjustStockAsync(Guid productId, int quantity, string reason);
    Task<InventoryItem> CreateInventoryItemAsync(InventoryItem item);
    Task UpdateInventoryItemAsync(InventoryItem item);
    Task<List<InventoryMovement>> GetMovementsForProductAsync(Guid productId, int limit = 10);
}

public class InventoryService : IInventoryService
{
    private readonly InventoryContext _context;
    private readonly IEventProducer _eventProducer;
    private readonly ILogger<InventoryService> _logger;
    private readonly ICacheService _cacheService;
    private const string TOPIC_NAME = "inventory-events";

    public InventoryService(InventoryContext context, IEventProducer eventProducer, ICacheService cacheService, ILogger<InventoryService> logger)
    {
        _context = context;
        _eventProducer = eventProducer;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<bool> AdjustStockAsync(Guid productId, int newQuantity, string reason)
    {
        string cacheKey = $"inventory:{productId}";
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId);

            if (item == null)
            {
                _logger.LogWarning($"Product {productId} not found in inventory");
                return false;
            }

            int quantityChange = newQuantity - item.QuantityAvailable;
            item.QuantityAvailable = newQuantity;

            if (newQuantity > 0)
            {
                item.LastRestockDate = DateTime.UtcNow;
            }

            var movement = new InventoryMovement
            {
                InventoryItemId = item.Id,
                Quantity = quantityChange,
                Type = quantityChange > 0 ? InventoryMovementType.StockIn : InventoryMovementType.StockOut,
                ReferenceType = "Adjustment",
                Timestamp = DateTime.UtcNow,
                InitiatedBy = "System"
            };

            _context.InventoryMovements.Add(movement);
            await _context.SaveChangesAsync();

            // Update cache
            await _cacheService.SetAsync(cacheKey, item, TimeSpan.FromMinutes(5));

            await _eventProducer.ProduceAsync(TOPIC_NAME, new InventoryChangedEvent
            {
                ProductId = productId,
                Sku = item.Sku,
                NewQuantityAvailable = item.QuantityAvailable,
                NewQuantityReserved = item.QuantityReserved,
                ChangeReason = reason
            });

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adjusting stock: {ex.Message}");
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<InventoryItem> CreateInventoryItemAsync(InventoryItem item)
    {
        string cacheKey = $"inventory:{item.ProductId}";

        item.Id = Guid.NewGuid();

        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync();

        // Cache the newly created item
        await _cacheService.SetAsync(cacheKey, item, TimeSpan.FromMinutes(5));

        await _eventProducer.ProduceAsync(TOPIC_NAME, new InventoryChangedEvent
        {
            ProductId = item.ProductId,
            Sku = item.Sku,
            NewQuantityAvailable = item.QuantityAvailable,
            NewQuantityReserved = item.QuantityReserved,
            ChangeReason = "New inventory item created"
        });

        return item;

    }

    public async Task<InventoryItem> GetByProductIdAsync(Guid productId)
    {
        var cacheKey = $"inventory:{productId}";

        // Try to get from cache first
        var cachedItem = await _cacheService.GetAsync<InventoryItem>(cacheKey);
        if (cachedItem != null)
        {
            _logger.LogInformation($"Cache hit for product {productId}. Returning cached item.");
            return cachedItem;
        }

        // If not in cache, get from database
        var item = await _context.InventoryItems
            .AsNoTracking() // Use AsNoTracking for read-only operations
            .FirstOrDefaultAsync(i => i.ProductId == productId);

        // Don't include the movements directly, they'll be loaded separately when needed

        // Cache the result if found
        if (item != null)
        {
            await _cacheService.SetAsync(cacheKey, item, TimeSpan.FromMinutes(5));
        }

        return item;
    }

    public async Task<bool> ReleaseStockAsync(Guid productId, int quantity, Guid orderId)
    {
        string cacheKey = $"inventory:{productId}";
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId);

            if (item == null)
            {
                _logger.LogWarning($"Product {productId} not found in inventory");
                return false;
            }

            item.QuantityAvailable += quantity;
            item.QuantityReserved -= quantity;

            var movement = new InventoryMovement
            {
                InventoryItemId = item.Id,
                Quantity = quantity,
                Type = InventoryMovementType.ReservationRelease,
                ReferenceId = orderId,
                ReferenceType = "Order",
                Timestamp = DateTime.UtcNow,
                InitiatedBy = "System"
            };

            _context.InventoryMovements.Add(movement);
            await _context.SaveChangesAsync();

            // Update cache
            await _cacheService.SetAsync(cacheKey, item, TimeSpan.FromMinutes(5));

            await _eventProducer.ProduceAsync(TOPIC_NAME, new InventoryChangedEvent
            {
                ProductId = productId,
                Sku = item.Sku,
                NewQuantityAvailable = item.QuantityAvailable,
                NewQuantityReserved = item.QuantityReserved,
                ChangeReason = $"Reservation released for order {orderId}"
            });

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error releasing stock: {ex.Message}");
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> ReserveStockAsync(Guid productId, int quantity, Guid orderId)
    {
        string cacheKey = $"inventory:{productId}";
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId);

            if (item == null)
            {
                _logger.LogWarning($"Product {productId} not found in inventory");
                return false;
            }

            if (item.QuantityAvailable < quantity)
            {
                _logger.LogWarning($"Insufficient stock for product {productId}. Available: {item.QuantityAvailable}, Requested: {quantity}");
                return false;
            }

            item.QuantityAvailable -= quantity;
            item.QuantityReserved += quantity;

            var movement = new InventoryMovement
            {
                InventoryItemId = item.Id,
                Quantity = quantity,
                Type = InventoryMovementType.Reservation,
                ReferenceId = orderId,
                ReferenceType = "Order",
                Timestamp = DateTime.UtcNow,
                InitiatedBy = "System"
            };

            _context.InventoryMovements.Add(movement);
            await _context.SaveChangesAsync();

            // Update cache
            await _cacheService.SetAsync(cacheKey, item, TimeSpan.FromMinutes(5));

            await _eventProducer.ProduceAsync(TOPIC_NAME, new InventoryReservedEvent
            {
                ProductId = productId,
                Sku = item.Sku,
                QuantityReserved = quantity,
                OrderId = orderId
            });

            // Check if stock lever is low after reservation
            if (item.QuantityAvailable <= item.ReorderThreshold)
            {
                await _eventProducer.ProduceAsync(TOPIC_NAME, new LowStockEvent
                {
                    ProductId = productId,
                    Sku = item.Sku,
                    CurrentStock = item.QuantityAvailable,
                    ReorderThreshold = item.ReorderThreshold,
                    RecommendedReorderQuantity = item.ReorderQuantity
                });
            }

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error reserving stock: {ex.Message}");
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateInventoryItemAsync(InventoryItem item)
    {
        string cacheKey = $"inventory:{item.ProductId}";

        _context.Entry(item).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        // Update cache
        await _cacheService.SetAsync(cacheKey, item, TimeSpan.FromMinutes(5));
    }

    public async Task<List<InventoryMovement>> GetMovementsForProductAsync(Guid productId, int limit = 10)
    {
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.ProductId == productId);

        if (item == null) return new List<InventoryMovement>();

        return await _context.InventoryMovements
            .Where(m => m.InventoryItemId == item.Id)
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .ToListAsync();
    }
}