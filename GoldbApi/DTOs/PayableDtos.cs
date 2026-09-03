namespace GoldbApi.DTOs;

public class PayableDto
{

    public int Id { get; set; }

    public int LogisticsCompanyId { get; set; }

    public string? LogisticsCompanyName { get; set; }

    public int ManufacturerCompanyId { get; set; }

    public string? ManufacturerCompanyName { get; set; }

    public int? OrderId { get; set; }

    public string? OrderNo { get; set; }

    public string? ProductName { get; set; }

    public string? ProductPhotoUrl { get; set; }

    public string Type { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public decimal RemainingAmount { get; set; }

    public decimal Weight { get; set; }

    public decimal RemainingWeight { get; set; }

    public string? Memo { get; set; }

    public string? SettlementMethod { get; set; }

    public decimal Discount { get; set; }

    public decimal DiscountWeight { get; set; }

    public bool IsCancelled { get; set; }

    public bool IsMfgProcessed { get; set; }

    public DateTime? MfgProcessedAt { get; set; }

    public int OrderCount { get; set; }

    // For a PAYMENT row: the current (as of now) unsettled RemainingAmount/RemainingWeight
    // summed across every charge this payment's PayableApplication rows touched. A payment
    // can be applied without fully covering a charge (partial settlement), so a payment with
    // Amount > 0 doesn't necessarily mean the charge(s) it paid are fully closed - this lets
    // the UI distinguish "fully settled" from "partially settled" instead of assuming 완료
    // just because the payment itself isn't cancelled.
    public decimal OutstandingChargeAmount { get; set; }

    public decimal OutstandingChargeWeight { get; set; }

    // True only for a PAYMENT row that has no later (non-cancelled) PAYMENT for the same
    // MFG/DCC pair - editing an older payment after a newer one already exists would change
    // amounts that later chronological ledger calculations (getLedgerBefore, etc.) already
    // built on top of, so only the most recent payment stays editable.
    public bool IsMostRecentPayment { get; set; }

    public DateTime CreatedAt { get; set; }

    // True for a PAYMENT created via 미수금 관리's 정산처리 (collecting an already-charged,
    // already-touched balance) rather than a fresh sale straight out of 정산 내역's worklist -
    // the frontend uses this to print a plain balance-only 거래명세서 with no product table,
    // since this transaction never introduced a new product charge.
    public bool IsOverdueSettlement { get; set; }
}

public class CompanyPayableSummaryDto
{

    public int CompanyId { get; set; }

    public string? CompanyName { get; set; }

    public decimal TotalCharge { get; set; }

    public decimal TotalPaid { get; set; }

    public decimal TotalOutstanding { get; set; }

    public decimal TotalChargeWeight { get; set; }

    public decimal TotalPaidWeight { get; set; }

    public decimal TotalOutstandingWeight { get; set; }

    public DateTime? LastPaymentDate { get; set; }
}

public class PayableQueryDto
{

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public int? CompanyId { get; set; }

    public string? Type { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? ProductName { get; set; }

    public string? OrderNo { get; set; }

    // Searches the order item's own memo (표시되는 "비고" 검색) - matches the reference
    // site's 정산 내역 filter, which searches by memo rather than product name.
    public string? Remarks { get; set; }
}

public class PayableOrderRowDto
{

    public int PayableId { get; set; }

    public int? OrderId { get; set; }

    public string? OrderNo { get; set; }

    public string? ProductName { get; set; }

    public string? ProductPhotoUrl { get; set; }

    public int ProductItemCount { get; set; }

    public List<PayableOrderItemSummaryDto> Items { get; set; } = new();

    // First non-empty item memo on the order - shown as its own 비고 column in the
    // worklist, matching what the 비고 search filter (oi.Memo) actually searches.
    public string? Remarks { get; set; }

    public int LogisticsCompanyId { get; set; }

    public string? LogisticsCompanyName { get; set; }

    public int ManufacturerCompanyId { get; set; }

    public string? ManufacturerCompanyName { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal ChargeAmount { get; set; }

    public decimal ChargeWeight { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal PaidWeight { get; set; }

    public decimal RemainingAmount { get; set; }

    public decimal RemainingWeight { get; set; }
}

public class PayableOrderItemSummaryDto
{

    public string? ProductName { get; set; }

    public string? ProductNo { get; set; }

    public string? PhotoUrl { get; set; }

    public string? Purity { get; set; }

    public string? Color { get; set; }

    public string? Size { get; set; }

    public string? Memo { get; set; }

    public int Quantity { get; set; }

    public decimal? ActualWeight { get; set; }

    public decimal MaterialCost { get; set; }

    public decimal LaborCost { get; set; }
}

public class PaymentApplicationDetailDto
{

    public int Id { get; set; }

    public int ChargeId { get; set; }

    public int? OrderId { get; set; }

    public string? OrderNo { get; set; }

    public string? ProductName { get; set; }

    public string? ProductPhotoUrl { get; set; }

    public int ProductItemCount { get; set; }

    public List<PayableOrderItemSummaryDto> Items { get; set; } = new();

    public decimal AppliedAmount { get; set; }

    public decimal AppliedWeight { get; set; }

    public decimal ChargeAmount { get; set; }

    public decimal ChargeWeight { get; set; }

    public decimal ChargeRemainingAmount { get; set; }

    public decimal ChargeRemainingWeight { get; set; }
}

public class UpdatePaymentApplicationDto
{

    public decimal AppliedAmount { get; set; }

    public decimal AppliedWeight { get; set; }
}

public class ChargeApplicationRowDto
{

    public int PaymentId { get; set; }

    public decimal AppliedAmount { get; set; }

    public decimal AppliedWeight { get; set; }

    public DateTime PaymentCreatedAt { get; set; }

    public bool IsCancelled { get; set; }

    public string? Memo { get; set; }

    // The payment's OWN recorded amount/weight/discount (as opposed to AppliedAmount/
    // AppliedWeight above, which is only the portion allocated to THIS charge) - needed so
    // this row is self-sufficient for editing the payment directly from a charge-centric
    // 주문별 보기 view, without a second fetch. Mirrors ReceivableChargeApplicationRowDto.
    public decimal Amount { get; set; }

    public decimal Weight { get; set; }

    public decimal Discount { get; set; }

    public decimal DiscountWeight { get; set; }
}

public class PayableOrderHistorySummaryDto
{

    public decimal TotalChargeAmount { get; set; }

    public decimal TotalChargeWeight { get; set; }

    public decimal TotalPaidAmount { get; set; }

    public decimal TotalPaidWeight { get; set; }
}

public class PayableOverdueQueryDto
{

    public int? LogisticsCompanyId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}

// 미수금 관리 (MFG's own overdue-collections tracker) - one row per DCC counterparty that
// has received at least one partial payment on a charge but still owes something. Separate
// from the plain 정산처리 worklist (untouched charges only) and from 정산 완료 내역 (a flat
// settlement history) - this is specifically for following up on partially-collected debt.
public class PayableOverdueRowDto
{

    public int LogisticsCompanyId { get; set; }

    public string? CompanyName { get; set; }

    // Oldest still-outstanding charge's own date - what 연체 일수 counts forward from.
    public DateTime SaleDate { get; set; }

    // Most recent activity (charge OR payment, whichever is later) across this counterparty's
    // whole ledger, not just the still-outstanding charges - lets 미수금 관리 show "마지막거래일자"
    // (last transaction date) distinct from SaleDate's "oldest overdue charge" meaning.
    public DateTime? LastTransactionDate { get; set; }

    public decimal SaleAmount { get; set; }

    public decimal SaleWeight { get; set; }

    public decimal CollectedAmount { get; set; }

    public decimal CollectedWeight { get; set; }

    public decimal OutstandingAmount { get; set; }

    public decimal OutstandingWeight { get; set; }

    public int OverdueDays { get; set; }

    // Feeds the existing BatchSettleDialog's order-ids prop for the "입금처리" action.
    public List<int> OrderIds { get; set; } = new();
}

public class CreatePaymentDto
{

    public int CompanyId { get; set; }

    public int? OrderId { get; set; }

    public List<int>? OrderIds { get; set; }

    public decimal Amount { get; set; }

    public decimal? Weight { get; set; }

    public decimal? Discount { get; set; }

    public decimal? DiscountWeight { get; set; }

    public string? Memo { get; set; }

    public string? SettlementMethod { get; set; }
}

public class MfgProcessDto
{

    public string? Memo { get; set; }
}

public class PurityBreakdownDto
{

    public string Purity { get; set; } = string.Empty;

    public decimal Weight { get; set; }

    public decimal Amount { get; set; }
}

public class OrderChargeSummaryDto
{

    public int CompanyId { get; set; }

    public string? CompanyName { get; set; }

    public decimal TotalOutstanding { get; set; }

    public decimal TotalOutstandingWeight { get; set; }

    public DateTime? LastPaymentDate { get; set; }

    public decimal SaleAmount { get; set; }

    public decimal SaleWeight { get; set; }

    public List<PurityBreakdownDto> PurityBreakdown { get; set; } = new();

    // One row per selected order/charge, for the settlement dialog's per-order table
    // (order info + editable 공임비) - mirrors the old reference site's 정산 내용 screen.
    public List<OrderChargeSummaryItemDto> Items { get; set; } = new();
}

public class OrderChargeSummaryItemDto
{

    public int PayableId { get; set; }

    public int? OrderId { get; set; }

    public string? OrderNo { get; set; }

    public string? ProductName { get; set; }

    public string? ProductNo { get; set; }

    public string? ProductPhotoUrl { get; set; }

    public string? Purity { get; set; }

    public string? Color { get; set; }

    public string? Size { get; set; }

    public string? Memo { get; set; }

    public int Quantity { get; set; }

    public decimal ChargeWeight { get; set; }

    public decimal? ActualWeight { get; set; }

    public decimal RemainingAmount { get; set; }

    public DateTime OrderDate { get; set; }

    public string? ManufacturerName { get; set; }

    public string? LogisticsCompanyName { get; set; }

    // An order can bundle several distinct products, not just one - the singular
    // ProductName/Purity/Quantity/ActualWeight fields above only ever reflected the first
    // top-level item, silently dropping the rest for a multi-product order. This carries
    // every top-level item so the settlement dialog can list them all, same as the plain
    // order-history table already does.
    public List<PayableOrderItemSummaryDto> Items { get; set; } = new();
}

public class OrderIdsQueryDto
{

    public List<int> OrderIds { get; set; } = new();
}

public class UpdatePaymentDto
{

    public decimal Amount { get; set; }

    public decimal? Weight { get; set; }

    public decimal? Discount { get; set; }

    public decimal? DiscountWeight { get; set; }

    public string? Memo { get; set; }

    public string? SettlementMethod { get; set; }
}

public class LedgerBalanceDto
{

    // Balance carried from the previous payment (or 0 if this is the first ever) - does NOT
    // include any charge that came in after that payment.
    public decimal BeforeAmount { get; set; }

    public decimal BeforeWeight { get; set; }

    // Charges that arrived after the previous payment and before this one - shown as its own
    // line (판매/B) instead of being silently folded into BeforeAmount.
    public decimal NewChargeAmount { get; set; }

    public decimal NewChargeWeight { get; set; }
}
