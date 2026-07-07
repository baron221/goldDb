using System.ComponentModel.DataAnnotations;

namespace GoldbApi.DTOs;

public class ProductDto
{

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ProductNo { get; set; }

    public string? Purity { get; set; }

    public int? CompanyId { get; set; }

    public string? CompanyName { get; set; }

    [Required(ErrorMessage = "대분류는 필수 항목입니다.")]
    public string CategoryLarge { get; set; } = string.Empty;

    public string? CategoryMedium { get; set; }

    public string? CategorySmall { get; set; }

    public decimal FactoryPrice { get; set; }

    public decimal Weight { get; set; }

    public string? ProductSize { get; set; }

    public decimal BasicLoss { get; set; }

    public string? CenterStone { get; set; }

    public string? CenterStoneShape { get; set; }

    public string? CenterStoneSize { get; set; }

    public int CenterStoneCount { get; set; }

    public string? SideStone { get; set; }

    public string? SideStoneShape { get; set; }

    public string? SideStoneSize { get; set; }

    public int SideStoneCount { get; set; }

    public string? Description { get; set; }

    public string? SpecialNote { get; set; }

    public bool IsPublic { get; set; }

    public decimal LaborCost { get; set; }

    public string? Colors { get; set; }

    public string? Sizes { get; set; }

    public List<ProductPhotoDto> Photos { get; set; } = new();

    public List<ProductOptionWeightDto> OptionWeights { get; set; } = new();
}

public class ProductPhotoDto
{

    public int Id { get; set; }

    public string PhotoUrl { get; set; } = string.Empty;

    public int? AttachmentId { get; set; }

    public bool IsMain { get; set; }

    public int SortOrder { get; set; }
}

public class CreateProductDto
{

    public string Name { get; set; } = string.Empty;

    public string? ProductNo { get; set; }

    [Required(ErrorMessage = "함량은 필수 선택 항목입니다.")]
    public string? Purity { get; set; }

    public int? CompanyId { get; set; }

    [Required(ErrorMessage = "대분류는 필수 항목입니다.")]
    public string CategoryLarge { get; set; } = string.Empty;

    public string? CategoryMedium { get; set; }

    public string? CategorySmall { get; set; }

    public decimal FactoryPrice { get; set; }

    public decimal Weight { get; set; }

    public string? ProductSize { get; set; }

    public decimal BasicLoss { get; set; }

    public string? CenterStone { get; set; }

    public string? CenterStoneShape { get; set; }

    public string? CenterStoneSize { get; set; }

    public int CenterStoneCount { get; set; }

    public string? SideStone { get; set; }

    public string? SideStoneShape { get; set; }

    public string? SideStoneSize { get; set; }

    public int SideStoneCount { get; set; }

    public string? Description { get; set; }

    public string? SpecialNote { get; set; }

    public bool IsPublic { get; set; }

    public decimal LaborCost { get; set; }

    [Required(ErrorMessage = "제품 컬러는 필수 선택 항목입니다.")]
    public string? Colors { get; set; }

    public string? Sizes { get; set; }

    public List<ProductPhotoRequestDto> Photos { get; set; } = new();

    public List<ProductOptionWeightDto> OptionWeights { get; set; } = new();
}

public class ProductQueryDto
{

    public string? Name { get; set; }

    public string? ProductNo { get; set; }

    public decimal? MinWeight { get; set; }

    public decimal? MaxWeight { get; set; }

    public string? CategoryLarge { get; set; }

    public string? CategoryMedium { get; set; }

    public string? CategorySmall { get; set; }

    public int? CompanyId { get; set; }

    public bool? IsPublic { get; set; }

    public string? Purity { get; set; }

    public int? MinSize { get; set; }

    public int? MaxSize { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

public class ProductPhotoRequestDto
{

    public int? Id { get; set; }

    public string? PhotoUrl { get; set; }

    public int? AttachmentId { get; set; }

    public bool IsMain { get; set; }

    public int SortOrder { get; set; }
}

public class ProductOptionWeightDto
{

    public int Id { get; set; }

    public string Purity { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public decimal Weight { get; set; }
}
