namespace GoldbApi.DTOs;

public class OrderDto
{

    public int Id { get; set; }

    public string OrderNo { get; set; } = string.Empty;

    public string? GroupOrderNo { get; set; }

    public int UserId { get; set; }

    public int? CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public string? UserName { get; set; }

    public string? UserDisplayName { get; set; }

    public string? CompanyName { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = "ORDERED";

    public string? OrderMemo { get; set; }

    public string? FactoryRemarks { get; set; }

    public string? LogisticsRemarks { get; set; }

    public string? InspectionRemarks { get; set; }

    public string? WorkOrderRemarks { get; set; }

    public string? CancellationReason { get; set; }

    public string? SettlementMethod { get; set; }

    public string? SettlementRemarks { get; set; }

    public string? SettlementStartMemo { get; set; }

    public decimal? SettlementAmount { get; set; }

    public string? ModificationMemo { get; set; }

    public string? CloseMemo { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public int? LogisticsCompanyId { get; set; }

    public string? LogisticsCompanyName { get; set; }

    public string? LogisticsCompanyBusinessNo { get; set; }

    public string? LogisticsCompanyAddress { get; set; }

    public string? LogisticsCompanyCEO { get; set; }

    public string? LogisticsCompanyPhone { get; set; }

    public string? CompanyPhone { get; set; }

    public string? CompanyBusinessNo { get; set; }

    public string? CompanyAddress { get; set; }

    public string? CompanyCEO { get; set; }

    public string? CompanyBusinessType { get; set; }

    public string? CompanyBusinessCategory { get; set; }

    public string? ManufacturerName { get; set; }

    public bool HasStatement { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<OrderItemDto> OrderItems { get; set; } = new();

    public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new();
}

public class OrderItemDto
{

    public int Id { get; set; }

    public int? ProductId { get; set; }

    public string? ProductName { get; set; }

    public string? ProductNo { get; set; }

    public string? Size { get; set; }

    public string? PhotoUrl { get; set; }

    public int? ProductSetId { get; set; }

    public string? ProductSetTitle { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal FactoryPrice { get; set; }

    public decimal LaborCost { get; set; }

    public decimal? FactoryInputMaterialCost { get; set; }

    public decimal? FactoryInputLaborCost { get; set; }

    public decimal? RetailerConfirmMaterialCost { get; set; }

    public decimal? RetailerConfirmLaborCost { get; set; }

    public DateTime? ProductionDate { get; set; }

    public decimal? Weight { get; set; }

    public decimal? OrderWeight { get; set; }

    public decimal? ActualWeight { get; set; }

    public string? InspectionMemo { get; set; }

    public string? Purity { get; set; }

    public string? Color { get; set; }

    public bool IsAsOrder { get; set; }

    public decimal? ConfirmedWeight { get; set; }

    public string? LogisticsMemo { get; set; }

    public decimal? ApprovedWeight { get; set; }

    public string? ApprovedMemo { get; set; }

    public decimal? RequestedWeight { get; set; }

    public string? RequestedMemo { get; set; }

    public decimal SettlementRatio { get; set; } = 70;

    public decimal? SettlementAmount { get; set; }

    public string? SettlementMemo { get; set; }

    public string? ManufacturerName { get; set; }

    public bool IsStockExhausted { get; set; }

    public string? StockNo { get; set; }

    public string? CategoryLarge { get; set; }

    public string? CategoryMedium { get; set; }

    public string? CategorySmall { get; set; }

    public List<OrderItemDto> Children { get; set; } = new();
}

public class CreateOrderDto
{

    public List<int> CartItemIds { get; set; } = new();

    public string? OrderMemo { get; set; }

    public string? FactoryRemarks { get; set; }

    public string? LogisticsRemarks { get; set; }

    public int? CustomerId { get; set; }

    public int? TargetCompanyId { get; set; }

    public int? DirectProductId { get; set; }

    public int? DirectProductSetId { get; set; }

    public int? DirectQuantity { get; set; }

    public string? DirectPurity { get; set; }

    public string? DirectColor { get; set; }
}

public class SettlementHistorySummaryDto
{

    public decimal TotalActualWeight { get; set; }

    public decimal TotalFineGold { get; set; }

    public decimal TotalSettlementAmount { get; set; }
}

public class OrderQueryDto
{

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Status { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? OrderNo { get; set; }

    public string? UserName { get; set; }

    public int? CustomerId { get; set; }

    public int? CompanyId { get; set; }

    public int? LogisticsCompanyId { get; set; }

    public int? FactoryCompanyId { get; set; }

    public bool? ExcludeCancelled { get; set; }

    public bool? ExcludeCompleted { get; set; }

    public bool? IsAsOnly { get; set; }

    public string? CategoryLarge { get; set; }

    public string? CategoryMedium { get; set; }

    public string? CategorySmall { get; set; }

    public string? SetCategoryLarge { get; set; }

    public string? SetCategoryMedium { get; set; }

    public string? SetCategorySmall { get; set; }

    public string? SortBy { get; set; } = "CreatedAt";

    public bool? IsDescending { get; set; } = true;
}

public class UpdateOrderStatusDto
{

    public string Status { get; set; } = string.Empty;

    public string? FactoryRemarks { get; set; }

    public string? InspectionRemarks { get; set; }

    public string? WorkOrderRemarks { get; set; }

    public string? CancellationReason { get; set; }

    public string? SettlementMethod { get; set; }

    public string? SettlementRemarks { get; set; }

    public string? SettlementStartMemo { get; set; }

    public decimal? SettlementAmount { get; set; }

    public string? ModificationMemo { get; set; }

    public string? CloseMemo { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public int? LogisticsCompanyId { get; set; }

    public List<OrderItemWeightDto>? ItemWeights { get; set; }
}

public class OrderItemWeightDto
{

    public int OrderItemId { get; set; }

    public decimal? ActualWeight { get; set; }

    public string? InspectionMemo { get; set; }

    public string? Purity { get; set; }

    public bool? IsAsOrder { get; set; }

    public decimal? ConfirmedWeight { get; set; }

    public decimal? OrderWeight { get; set; }

    public string? LogisticsMemo { get; set; }

    public decimal? ApprovedWeight { get; set; }

    public string? ApprovedMemo { get; set; }

    public decimal? RequestedWeight { get; set; }

    public string? RequestedMemo { get; set; }

    public decimal? SettlementRatio { get; set; }

    public decimal? SettlementAmount { get; set; }

    public string? SettlementMemo { get; set; }

    public decimal? FactoryPrice { get; set; }

    public decimal? LaborCost { get; set; }

    public decimal? FactoryInputMaterialCost { get; set; }

    public decimal? FactoryInputLaborCost { get; set; }

    public decimal? RetailerConfirmMaterialCost { get; set; }

    public decimal? RetailerConfirmLaborCost { get; set; }

    public DateTime? ProductionDate { get; set; }
}

public class OrderStatusHistoryDto
{

    public int Id { get; set; }

    public int OrderId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? StatusName { get; set; }

    public int? UserId { get; set; }

    public string? UserName { get; set; }

    public string? UserDisplayName { get; set; }

    public string? CompanyName { get; set; }

    public string? CompanyType { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class OrderStatementDto
{

    public int OrderId { get; set; }

    public string SnapshotData { get; set; } = string.Empty;

    public byte[]? PdfBinary { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class SaveOrderStatementDto
{

    public int OrderId { get; set; }

    public string SnapshotData { get; set; } = string.Empty;

    public string? PdfBase64 { get; set; }
}
