using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoldbApi.Models;

[Description("주문 상태 변경 이력")]
public class OrderStatusHistory : BaseModel
{

    [Key]
    [Description("ID")]

    public int Id { get; set; }

    [Required]
    [Description("주문 ID")]

    public int OrderId { get; set; }

    public Order? Order { get; set; }

    [Required]
    [MaxLength(50)]
    [Description("변경된 주문 상태")]

    public string Status { get; set; } = string.Empty;

    [Description("처리자 사용자 ID")]
    public int? UserId { get; set; }

    public User? User { get; set; }

    [MaxLength(500)]
    [Description("메모")]

    public string? Remarks { get; set; }
}
