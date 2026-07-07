namespace GoldbApi.DTOs;

public class CatalogDto
{

    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string SecurityClass { get; set; } = "일반";

    public string IssueNo { get; set; } = string.Empty;

    public string PhotoUrl { get; set; } = string.Empty;

    public int TotalPages { get; set; }

    public List<CatalogPageDto> Pages { get; set; } = new();
}

public class ProductListItemDto
{

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ProductNo { get; set; } = string.Empty;
}

public class CreateCatalogDto
{

    public string Title { get; set; } = string.Empty;

    public string SecurityClass { get; set; } = "일반";

    public string IssueNo { get; set; } = string.Empty;

    public string PhotoUrl { get; set; } = string.Empty;

    public int TotalPages { get; set; }

    public List<SaveCatalogPageDto> Pages { get; set; } = new();
}

public class CatalogQueryDto
{

    public string? Title { get; set; }

    public string? SecurityClass { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

public class CatalogPageDto
{
    public int Id { get; set; }
    public int PageNumber { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public string CategoryLarge { get; set; } = string.Empty;
    public string CategoryMedium { get; set; } = string.Empty;
    public string CategorySmall { get; set; } = string.Empty;

    public List<ProductListItemDto> Products { get; set; } = new();
}

public class SaveCatalogPageDto
{
    public int? Id { get; set; }
    public int PageNumber { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? CompanyId { get; set; }
    public string CategoryLarge { get; set; } = string.Empty;
    public string CategoryMedium { get; set; } = string.Empty;
    public string CategorySmall { get; set; } = string.Empty;

    public List<int> ProductIds { get; set; } = new();
}
