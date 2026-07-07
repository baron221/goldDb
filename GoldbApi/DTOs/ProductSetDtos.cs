namespace GoldbApi.DTOs;

public class ProductSetDto
{

    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? SetNo { get; set; }

    public string? Description { get; set; }

    public List<ProductSetPhotoDto> Photos { get; set; } = new();

    public bool IsPublic { get; set; }

    public decimal FactoryPrice { get; set; }

    public decimal LaborCost { get; set; }

    public int? CompanyId { get; set; }

    public string? CompanyName { get; set; }

    public int BasicComponentCount { get; set; }

    public string? CategoryLarge { get; set; }

    public string? CategoryMedium { get; set; }

    public string? CategorySmall { get; set; }

    public List<ProductDto> Products { get; set; } = new();

    public DateTime CreatedAt { get; set; }
}

public class CreateProductSetDto
{

    public string Title { get; set; } = string.Empty;

    public string? SetNo { get; set; }

    public string? Description { get; set; }

    public List<ProductSetPhotoDto> Photos { get; set; } = new();

    public bool IsPublic { get; set; }

    public decimal FactoryPrice { get; set; }

    public decimal LaborCost { get; set; }

    public int? CompanyId { get; set; }

    public int BasicComponentCount { get; set; }

    public string? CategoryLarge { get; set; }

    public string? CategoryMedium { get; set; }

    public string? CategorySmall { get; set; }

    public List<int> ProductIds { get; set; } = new();
}

public class ProductSetQueryDto
{

    public string? Title { get; set; }

    public string? CategoryLarge { get; set; }

    public string? CategoryMedium { get; set; }

    public string? CategorySmall { get; set; }

    public bool? IsPublic { get; set; }

    public int? CompanyId { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

public class ProductSetPhotoDto
{

    public int Id { get; set; }

    public string? PhotoUrl { get; set; }

    public int? AttachmentId { get; set; }

    public bool IsMain { get; set; }

    public int SortOrder { get; set; }
}

public class ProductSetPhotoRequestDto
{

    public int? Id { get; set; }

    public string? PhotoUrl { get; set; }

    public int? AttachmentId { get; set; }

    public bool IsMain { get; set; }

    public int SortOrder { get; set; }
}
