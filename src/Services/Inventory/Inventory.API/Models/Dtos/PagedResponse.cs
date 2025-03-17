namespace Inventory.API.Models.Dtos;

public class PagedResponse<T>
{
    public IEnumerable<T> Items { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

    // Helper methods to initialize metadata
    public static PagedResponse<T> Create(IEnumerable<T> items, int count, int pageNumber, int pageSize)
    {
        var totalPages = (int)Math.Ceiling(count / (double)pageSize);

        var metadata = new Dictionary<string, object>
        {
            { "totalCount", count },
            { "pageSize", pageSize },
            { "currentPage", pageNumber },
            { "totalPages", totalPages },
            { "hasPrevious", pageNumber > 1 },
            { "hasNext", pageNumber < totalPages }
        };

        var response = new PagedResponse<T>
        {
            Items = items,
            Metadata = metadata
        };

        return response;
    }
}