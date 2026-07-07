using System.ComponentModel;

namespace GoldbApi.Models;

[Description("거래 내역 정보")]
public class Transaction : BaseModel
{

    [Description("거래 ID")]
    public int Id { get; set; }

    [Description("주문 번호")]
    public string OrderNo { get; set; } = string.Empty;

    [Description("타임스탬프")]
    public long Timestamp { get; set; }

    [Description("사용자명")]
    public string Username { get; set; } = string.Empty;

    [Description("가격")]
    public decimal Price { get; set; }

    [Description("상태")]
    public string Status { get; set; } = "success";
}
