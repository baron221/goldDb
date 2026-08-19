namespace GoldbApi.DTOs;

public class ReceivableDto
{

    public int Id { get; set; }

    public int UserId { get; set; }

    public string? UserName { get; set; }

    public string? UserDisplayName { get; set; }

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

    // Only populated for DEPOSIT rows - which charge(s) this payment actually paid
    // off, since a payment can spill over past its own order into other outstanding
    // charges (oldest first) once its tagged order is fully covered.
    public List<AppliedChargeDto> AppliedCharges { get; set; } = new();
}

public class AppliedChargeDto
{

    public int ChargeId { get; set; }

    public int? OrderId { get; set; }

    public string? OrderNo { get; set; }

    public decimal AppliedAmount { get; set; }

    public decimal AppliedWeight { get; set; }
}

public class UserReceivableSummaryDto
{

    public int UserId { get; set; }

    public string? UserName { get; set; }

    public string? UserDisplayName { get; set; }

    public string? CompanyName { get; set; }

    public decimal TotalCharge { get; set; }

    public decimal TotalDeposit { get; set; }

    public decimal TotalReceivable { get; set; }

    public decimal TotalChargeWeight { get; set; }

    public decimal TotalDepositWeight { get; set; }

    public decimal TotalReceivableWeight { get; set; }

    public DateTime? LastPaymentDate { get; set; }
}

public class PuritySummaryDto
{

    public decimal Purity14k { get; set; }

    public decimal Purity18k { get; set; }

    public decimal PureGold { get; set; }

    public decimal PurityPt { get; set; }
}

public class ReceivableQueryDto
{

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public int? UserId { get; set; }

    public string? Type { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? OrderNo { get; set; }

    public string? ProductNo { get; set; }

    public string? ProductName { get; set; }

    public int? ManufacturerId { get; set; }

    public string? CategoryLarge { get; set; }

    public string? CategoryMedium { get; set; }

    public string? CategorySmall { get; set; }
}

public class CreateDepositDto
{

    public int UserId { get; set; }

    public int? OrderId { get; set; }

    public decimal Amount { get; set; }

    public decimal? Weight { get; set; }

    public decimal? Discount { get; set; }

    public string? Memo { get; set; }

    public string? SettlementMethod { get; set; }
}

public class UpdateDepositDto
{

    public decimal Amount { get; set; }

    public decimal? Weight { get; set; }

    public decimal? Discount { get; set; }

    public string? Memo { get; set; }

    public string? SettlementMethod { get; set; }
}

public class LogisticsReceivableSummaryDto
{

    public decimal TotalUnpaid { get; set; }

    public decimal CurrentMonthCharge { get; set; }

    public decimal CurrentMonthDeposit { get; set; }

    public decimal CurrentMonthUnpaid { get; set; }

    public List<RetailerReceivableSummaryDto> RetailerSummaries { get; set; } = new();
}

public class RetailerReceivableSummaryDto
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public decimal TotalUnpaid { get; set; }
    public decimal CurrentMonthCharge { get; set; }
    public decimal CurrentMonthDeposit { get; set; }
}
