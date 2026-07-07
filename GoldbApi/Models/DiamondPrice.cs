using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoldbApi.Models;

[Description("다이아몬드 시세 정보")]
public class DiamondPrice : BaseModel
{

    [Key]
    [Description("ID")]

    public int Id { get; set; }

    [Description("가격 구분")]
    [Required]
    [MaxLength(20)]

    public string PriceType { get; set; } = string.Empty;

    [Description("사이즈")]
    [Required]
    [MaxLength(20)]

    public string Size { get; set; } = string.Empty;

    [Description("버진 가격")]
    public decimal VirginPrice { get; set; }

    [Description("우신 가격")]
    public decimal WooshinPrice { get; set; }

    [Description("현대 가격")]
    public decimal HyundaiPrice { get; set; }

    [Description("국제 가격")]
    public decimal GukjePrice { get; set; }

    [Description("국보 가격")]
    public decimal GukboPrice { get; set; }

    [Description("기타 가격")]
    public decimal OtherPrice { get; set; }

    [Description("고시 일시")]
    public DateTime AnnouncedAt { get; set; } = DateTime.UtcNow;
}
