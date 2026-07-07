using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoldbApi.Models;

[Description("세트 제품 정보")]
public class ProductSet : BaseModel
{

    [Key]
    [Description("ID")]

    public int Id { get; set; }

    [Description("세트 제목")]
    [Required]
    [MaxLength(200)]

    public string Title { get; set; } = string.Empty;

    [Description("세트 번호")]
    [MaxLength(100)]

    public string? SetNo { get; set; }

    [Description("세트 설명")]
    public string? Description { get; set; }

    [Description("세트 사진")]
    public string PhotoUrl { get; set; } = string.Empty;

    [Description("공개여부")]
    public bool IsPublic { get; set; } = false;

    [Description("공장도가격")]
    public decimal FactoryPrice { get; set; }

    [Description("수공비")]
    public decimal LaborCost { get; set; }

    [Description("생산업체 ID")]
    public int? CompanyId { get; set; }

    public Company? Company { get; set; }

    [Description("기본 구성품 수")]
    public int BasicComponentCount { get; set; }

    [Description("대분류")]
    [MaxLength(50)]

    public string? CategoryLarge { get; set; }

    [Description("중분류")]
    [MaxLength(50)]

    public string? CategoryMedium { get; set; }

    [Description("소분류")]
    [MaxLength(50)]

    public string? CategorySmall { get; set; }

    public ICollection<ProductSetItem> SetItems { get; set; } = new List<ProductSetItem>();

    public ICollection<ProductSetPhoto> ProductSetPhotos { get; set; } = new List<ProductSetPhoto>();
}

[Description("세트 구성 제품 정보")]
public class ProductSetItem : BaseModel
{

    [Description("세트 ID")]
    public int ProductSetId { get; set; }

    public ProductSet? ProductSet { get; set; }

    [Description("제품 ID")]
    public int ProductId { get; set; }

    public Product? Product { get; set; }
}

[Description("세트 제품 사진 정보")]
public class ProductSetPhoto : BaseModel
{

    [Key]
    [Description("ID")]

    public int Id { get; set; }

    [Description("세트 제품 ID")]
    public int ProductSetId { get; set; }

    public ProductSet? ProductSet { get; set; }

    [Description("사진 URL")]
    public string? PhotoUrl { get; set; }

    [Description("첨부파일 ID")]
    public int? AttachmentId { get; set; }

    public Attachment? Attachment { get; set; }

    [Description("대표 사진 여부")]
    public bool IsMain { get; set; } = false;

    [Description("정렬 순서")]
    public int SortOrder { get; set; }
}
