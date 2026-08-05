using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoldbApi.Models;

[Description("입금이 어느 청구건에 얼마나 적용됐는지 기록 (정산 내역 추적용)")]
public class ReceivableApplication : BaseModel
{
    [Key]

    public int Id { get; set; }

    [Required]

    public int DepositId { get; set; }

    public Receivable? Deposit { get; set; }

    [Required]

    public int ChargeId { get; set; }

    public Receivable? Charge { get; set; }

    public decimal AppliedAmount { get; set; }

    [Description("적용된 순금 중량(g)")]
    public decimal AppliedWeight { get; set; }
}
