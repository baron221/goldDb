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

    public DateTime CreatedAt { get; set; }
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
}

public class PayableOrderHistorySummaryDto
{

    public decimal TotalChargeAmount { get; set; }

    public decimal TotalChargeWeight { get; set; }

    public decimal TotalPaidAmount { get; set; }

    public decimal TotalPaidWeight { get; set; }
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
