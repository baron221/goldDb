using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoldbApi.Models;

[Description("물류-공장 정산 내역 (물류가 공장에 지급해야 할 금액)")]
public class Payable : BaseModel
{
    [Key]

    public int Id { get; set; }

    [Required]

    public int LogisticsCompanyId { get; set; }

    public Company? LogisticsCompany { get; set; }

    [Required]

    public int ManufacturerCompanyId { get; set; }

    public Company? ManufacturerCompany { get; set; }

    public int? OrderId { get; set; }

    public Order? Order { get; set; }

    [Required]
    [MaxLength(20)]

    public string Type { get; set; } = "CHARGE";

    public decimal Amount { get; set; }

    public decimal RemainingAmount { get; set; }

    [Description("순금 중량(g)")]
    public decimal Weight { get; set; }

    [Description("잔여 순금 중량(g)")]
    public decimal RemainingWeight { get; set; }

    [MaxLength(500)]

    public string? Memo { get; set; }

    [MaxLength(20)]

    public string? SettlementMethod { get; set; }

    [Description("할인 금액")]
    public decimal Discount { get; set; } = 0;

    [Description("할인 순금 중량(g)")]
    public decimal DiscountWeight { get; set; } = 0;

    [Description("취소 여부")]
    public bool IsCancelled { get; set; } = false;

    [Description("공장 정산 처리 여부 (실제 입금 확인이 아닌 공장측 확인 표시)")]
    public bool IsMfgProcessed { get; set; } = false;

    [Description("공장 정산 처리 일시")]
    public DateTime? MfgProcessedAt { get; set; }

    // 미수금 관리(이미 한 번 정산처리를 거친, 남은 잔액)에서 입금처리된 PAYMENT인지 여부.
    // true인 경우: (1) 기존 진행 중인 PAYMENT에 병합하지 않고 항상 새 거래(row)로 만들고,
    // (2) 거래명세서에 상품 내역을 표시하지 않는다 - 이 결제는 새로운 청구가 아니라
    // 이미 청구된 금액의 단순 정산이기 때문이다. 정산 내역(미청구 worklist)에서 바로
    // 만들어진 PAYMENT는 항상 false. Mirrors Receivable.IsOverdueSettlement.
    [Description("미수금 관리에서 발생한 단순 정산 여부")]
    public bool IsOverdueSettlement { get; set; } = false;
}
