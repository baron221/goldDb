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

    public bool IsCancelled { get; set; }

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
}

public class PayableOrderRowDto
{

    public int PayableId { get; set; }

    public int? OrderId { get; set; }

    public string? OrderNo { get; set; }

    public string? ProductName { get; set; }

    public string? ProductPhotoUrl { get; set; }

    public int ProductItemCount { get; set; }

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

    public decimal Amount { get; set; }

    public decimal? Weight { get; set; }

    public decimal? Discount { get; set; }

    public string? Memo { get; set; }

    public string? SettlementMethod { get; set; }
}

public class UpdatePaymentDto
{

    public decimal Amount { get; set; }

    public decimal? Weight { get; set; }

    public decimal? Discount { get; set; }

    public string? Memo { get; set; }

    public string? SettlementMethod { get; set; }
}
