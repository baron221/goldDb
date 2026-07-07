using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoldbApi.Models;

[Description("제품 정보")]
public class Product : BaseModel
{

    [Key]
    [Description("ID")]

    public int Id { get; set; }

    [Description("상품이름")]
    [Required]
    [MaxLength(200)]

    public string Name { get; set; } = string.Empty;

    [Description("정규화된 상품이름")]
    [Required]
    [MaxLength(200)]
    public string NormalizedName { get; set; } = string.Empty;

    [Description("제품번호")]
    [MaxLength(100)]

    public string? ProductNo { get; set; }

    [Description("정규화된 제품번호")]
    [MaxLength(100)]
    public string? NormalizedProductNo { get; set; }

    [Description("함량")]
    [MaxLength(50)]

    public string? Purity { get; set; }

    [Description("중량")]
    public decimal Weight { get; set; }

    [Description("생산업체 ID")]
    public int? CompanyId { get; set; }

    public Company? Company { get; set; }

    [Description("제품 대분류")]
    [Required]
    [MaxLength(50)]

    public string CategoryLarge { get; set; } = string.Empty;

    [Description("제품 중분류")]
    [MaxLength(50)]

    public string? CategoryMedium { get; set; }

    [Description("제품 소분류")]
    [MaxLength(50)]

    public string? CategorySmall { get; set; }

    [Description("공장도가격")]
    public decimal FactoryPrice { get; set; }

    [Description("디자인고시")]
    [MaxLength(500)]

    public string? DesignNotice { get; set; }

    [Description("제품크기")]
    [MaxLength(100)]

    public string? ProductSize { get; set; }

    [Description("기본감량")]
    public decimal BasicLoss { get; set; }

    [Description("중앙석")]
    [MaxLength(100)]

    public string? CenterStone { get; set; }

    [Description("중앙석형태")]
    [MaxLength(100)]

    public string? CenterStoneShape { get; set; }

    [Description("중앙석 사이즈")]
    [MaxLength(100)]

    public string? CenterStoneSize { get; set; }

    [Description("중앙석 개수")]
    public int CenterStoneCount { get; set; }

    [Description("보조석")]
    [MaxLength(100)]

    public string? SideStone { get; set; }

    [Description("보조석 형태")]
    [MaxLength(100)]

    public string? SideStoneShape { get; set; }

    [Description("보조석 사이즈")]
    [MaxLength(100)]

    public string? SideStoneSize { get; set; }

    [Description("보조석 개수")]
    public int SideStoneCount { get; set; }

    [Description("제품설명")]
    public string? Description { get; set; }

    [Description("특이사항")]
    [MaxLength(500)]

    public string? SpecialNote { get; set; }

    [Description("공개 여부")]
    public bool IsPublic { get; set; } = false;

    [Description("수공비")]
    public decimal LaborCost { get; set; }

    [Description("제품 컬러")]
    [MaxLength(200)]

    public string? Colors { get; set; }

    [Description("제품 사이즈")]
    [MaxLength(200)]

    public string? Sizes { get; set; }

    public ICollection<ProductPhoto> ProductPhotos { get; set; } = new List<ProductPhoto>();

    public ICollection<ProductOptionWeight> OptionWeights { get; set; } = new List<ProductOptionWeight>();
}

[Description("제품 사진 정보")]
public class ProductPhoto : BaseModel
{

    [Key]
    [Description("ID")]

    public int Id { get; set; }

    [Description("제품 ID")]
    public int ProductId { get; set; }

    public Product? Product { get; set; }

    [Description("사진 URL")]
    [Required]

    public string PhotoUrl { get; set; } = string.Empty;

    [Description("첨부파일 ID")]
    public int? AttachmentId { get; set; }

    public Attachment? Attachment { get; set; }

    [Description("대표 사진 여부")]
    public bool IsMain { get; set; } = false;

    [Description("정렬 순서")]
    public int SortOrder { get; set; }
}
