using GoldbApi.DTOs;

namespace GoldbApi.DTOs;

public class SearchQueryDto
{

    public string? Q { get; set; }

    public string? Search { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Type { get; set; }

    public string? CategoryLarge { get; set; }

    public string? CategoryMedium { get; set; }

    public string? CategorySmall { get; set; }

    public int? CompanyId { get; set; }
}

public class IntegratedSearchResultDto
{

    public List<ProductSearchResultDto> Products { get; set; } = new();

    public List<ProductSetSearchResultDto> ProductSets { get; set; } = new();

    public List<StockSearchResultDto> Stocks { get; set; } = new();

    public List<OrderSearchResultDto> Orders { get; set; } = new();

    public List<MarketItemDto> Items { get; set; } = new();

    public int Total { get; set; }
}

public class MarketItemDto
{

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? No { get; set; }

    public bool IsSet { get; set; }

    public string? CategoryName { get; set; }

    public string? PhotoUrl { get; set; }

    public decimal Price { get; set; }
}

public class ProductSearchResultDto
{

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ProductNo { get; set; }

    public string? CategoryName { get; set; }

    public string? PhotoUrl { get; set; }
}

public class ProductSetSearchResultDto
{

    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? SetNo { get; set; }

    public string? CategoryName { get; set; }

    public string? PhotoUrl { get; set; }
}

public class StockSearchResultDto
{

    public int Id { get; set; }

    public string StockNo { get; set; } = string.Empty;

    public string? ProductName { get; set; }

    public string? ProductNo { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? PhotoUrl { get; set; }
}

public class OrderSearchResultDto
{

    public int Id { get; set; }

    public string OrderNo { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string? UserName { get; set; }

    public string? CompanyName { get; set; }
}
