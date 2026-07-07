using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoldbApi.Models;

[Description("카탈로그 정보")]
public class Catalog : BaseModel
{

    [Key]
    [Description("ID")]

    public int Id { get; set; }

    [Description("책제목")]
    [Required]
    [MaxLength(200)]

    public string Title { get; set; } = string.Empty;

    [Description("보안분류")]
    [MaxLength(20)]

    public string SecurityClass { get; set; } = "일반";

    [Description("발행호")]
    [MaxLength(50)]

    public string IssueNo { get; set; } = string.Empty;

    [Description("책자사진")]
    public string PhotoUrl { get; set; } = string.Empty;

    [Description("전체페이지수")]
    public int TotalPages { get; set; }

    public ICollection<CatalogPage> CatalogPages { get; set; } = new List<CatalogPage>();
}
