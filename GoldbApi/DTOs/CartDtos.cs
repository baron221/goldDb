namespace GoldbApi.DTOs;

public class CartItemDto
{

    public int Id { get; set; }

    public int? ProductId { get; set; }

    public string? ProductName { get; set; }

    public string? ProductNo { get; set; }

    public string? CompanyName { get; set; }

    public string? PhotoUrl { get; set; }

    public int? ProductSetId { get; set; }

    public string? ProductSetTitle { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal? CustomFactoryPrice { get; set; }

    public decimal? CustomLaborCost { get; set; }

    public decimal FactoryPrice { get; set; }

    public decimal LaborCost { get; set; }

    public string? Purity { get; set; }

    public string? Color { get; set; }

    public decimal Weight { get; set; }

    public string? CategoryLarge { get; set; }

    public string? CategoryMedium { get; set; }

    public string? CategorySmall { get; set; }
}

public class AddToCartDto
{

    public int? ProductId { get; set; }

    public int? ProductSetId { get; set; }

    public int Quantity { get; set; } = 1;

    public string? Purity { get; set; }

    public string? Color { get; set; }

    public int? TargetCompanyId { get; set; }
}

public class UpdateCartQuantityDto
{

    public int Quantity { get; set; }
}

public class UpdateCartPriceDto
{

    public decimal? FactoryPrice { get; set; }

    public decimal? LaborCost { get; set; }
}
