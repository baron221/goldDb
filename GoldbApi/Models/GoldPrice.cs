using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoldbApi.Models;

[Description("일반 시세 정보")]
public class GoldPrice : BaseModel
{

    [Key]
    [Description("ID")]

    public int Id { get; set; }

    [Description("고시 일시")]
    public DateTime AnnouncedAt { get; set; } = DateTime.UtcNow;

    [Description("백금 시세")]
    public decimal Platinum { get; set; }

    [Description("순금 시세")]
    public decimal PureGold { get; set; }

    [Description("18K 시세")]
    public decimal Gold18K { get; set; }

    [Description("14K 시세")]
    public decimal Gold14K { get; set; }

    [Description("실버 시세")]
    public decimal Silver { get; set; }
}
