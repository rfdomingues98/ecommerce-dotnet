namespace Inventory.API.Models.Dtos;

public class InventoryQueryParametersDto
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Min(value, MaxPageSize);
    }
    public string? Sku { get; set; }
    public int? MinStock { get; set; }
    public int? MaxStock { get; set; }
    public bool? LowStockOnly { get; set; }
    public string SortBy { get; set; } = "Sku";
    public bool SortDescending { get; set; } = false;
}
