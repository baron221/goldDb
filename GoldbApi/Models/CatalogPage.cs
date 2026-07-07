using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoldbApi.Models;

[Description("카탈로그 페이지 정보")]
public class CatalogPage : BaseModel
{

    [Key]
    [Description("ID")]
    public int Id { get; set; }

    [Description("카탈로그 ID")]
    [Required]
    public int CatalogId { get; set; }

    public Catalog? Catalog { get; set; }

    [Description("페이지 번호")]
    [Required]
    public int PageNumber { get; set; }

    [Description("페이지 사진")]
    [MaxLength(500)]
    public string PhotoUrl { get; set; } = string.Empty;

    [Description("페이지 설명")]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Description("제조사 ID")]
    public int? CompanyId { get; set; }

    public Company? Company { get; set; }

    [Description("대분류")]
    [MaxLength(100)]
    public string CategoryLarge { get; set; } = string.Empty;

    [Description("중분류")]
    [MaxLength(100)]
    public string CategoryMedium { get; set; } = string.Empty;

    [Description("소분류")]
    [MaxLength(100)]
    public string CategorySmall { get; set; } = string.Empty;

    public ICollection<CatalogPageProduct> CatalogPageProducts { get; set; } = new List<CatalogPageProduct>();
}
