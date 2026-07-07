using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoldbApi.Models;

[Description("제품 옵션 조합별 중량 정보")]
public class ProductOptionWeight : BaseModel
{

    [Key]
    [Description("ID")]

    public int Id { get; set; }

    [Description("제품 ID")]
    public int ProductId { get; set; }

    public Product? Product { get; set; }

    [Required]
    [MaxLength(50)]
    [Description("함량")]

    public string Purity { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Description("컬러")]

    public string Color { get; set; } = string.Empty;

    [Description("중량")]
    public decimal Weight { get; set; }
}
