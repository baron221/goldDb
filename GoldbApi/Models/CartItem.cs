using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoldbApi.Models;

[Description("장바구니 아이템 정보")]
public class CartItem : BaseModel
{

    [Key]
    [Description("ID")]

    public int Id { get; set; }

    [Description("사용자 ID")]
    public int UserId { get; set; }

    public User? User { get; set; }

    [Description("제품 ID")]
    public int? ProductId { get; set; }

    public Product? Product { get; set; }

    [Description("세트 제품 ID")]
    public int? ProductSetId { get; set; }

    public ProductSet? ProductSet { get; set; }

    [Description("수량")]
    public int Quantity { get; set; } = 1;

    [Description("함량")]
    [MaxLength(20)]

    public string? Purity { get; set; }

    [Description("컬러")]
    [MaxLength(50)]

    public string? Color { get; set; }

    [Description("사이즈")]
    [MaxLength(50)]
    public string? Size { get; set; }

    [Description("주문 메모")]
    [MaxLength(500)]
    public string? Memo { get; set; }

    [Description("사용자 정의 공장도 가격")]
    public decimal? CustomFactoryPrice { get; set; }

    [Description("사용자 정의 수공비")]
    public decimal? CustomLaborCost { get; set; }
}
