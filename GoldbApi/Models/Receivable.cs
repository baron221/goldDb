using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoldbApi.Models;

[Description("미수금 내역")]
public class Receivable : BaseModel
{
    [Key]

    public int Id { get; set; }

    [Required]

    public int UserId { get; set; }

    public User? User { get; set; }

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

    [Description("이 입금을 발생시킨 Payable(정산처리) 결제 ID - MFG-DCC 정산이 이 주문의 미수금에 비례 반영된 경우에만 값이 있음")]
    public int? SourcePaymentId { get; set; }

    // 미수금 관리(이미 한 번 정산처리를 거친, 남은 잔액)에서 입금처리된 DEPOSIT인지 여부.
    // true인 경우: (1) 기존 진행 중인 DEPOSIT에 병합하지 않고 항상 새 거래(row)로 만들고,
    // (2) 거래명세서에 상품 내역을 표시하지 않는다 - 이 입금은 새로운 판매가 아니라
    // 이미 청구된 금액의 단순 수금이기 때문이다. 정산 대상 내역(미청구 worklist)에서
    // 바로 만들어진 DEPOSIT은 항상 false.
    [Description("미수금 관리에서 발생한 단순 수금 여부")]
    public bool IsOverdueSettlement { get; set; } = false;
}
