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

    public int? HandledByUserId { get; set; }

    public string? HandledByUserName { get; set; }

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

    public bool IsPayableSettled { get; set; }

    // True once every one of this order's Payable charges (one per manufacturer involved)
    // has at least one PayableApplication on file - the exact condition
    // ReceivableService.GetManufacturerSettlementBacklogBlockAsync gates 물류도착 on. Unlike
    // IsPayableSettled (which requires the charge to be fully PAID), a 0-value 정산처리
    // acknowledgement already satisfies this. Lets 물류승인내역 hide/disable the 물류도착
    // button up front instead of letting the user hit the same backend rejection.
    public bool IsSettlementProcessed { get; set; }

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

    public string? Memo { get; set; }

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

    public decimal BasicLoss { get; set; }

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

    public int? HandledByUserId { get; set; }

    public int? DirectProductId { get; set; }

    public int? DirectProductSetId { get; set; }

    public int? DirectQuantity { get; set; }

    public string? DirectPurity { get; set; }

    public string? DirectColor { get; set; }

    public string? DirectSize { get; set; }

    public string? DirectMemo { get; set; }

    // Per-order override of the product's own baked-in cost, for 주문 수기 등록 (manual
    // order registration) - mirrors CartItem.CustomFactoryPrice/CustomLaborCost. Left null
    // to fall back to the selected product's own FactoryPrice/LaborCost, same as any other
    // direct order.
    public decimal? DirectFactoryPrice { get; set; }

    public decimal? DirectLaborCost { get; set; }

    // 주문 수기 등록에서 카탈로그에 없는 제품을 자유 입력으로 등록할 때 쓰인다.
    // DirectProductId/DirectProductSetId가 둘 다 없을 때만 유효하며, 그 경우
    // DirectWeight도 함께 입력받아야 한다 (카탈로그 제품이 없으니 중량을 산정할
    // Product.OptionWeights/Weight가 없다).
    public string? DirectProductName { get; set; }

    public decimal? DirectWeight { get; set; }

    // Required alongside DirectProductName: with no catalog product there's no
    // Product.CompanyId to derive the manufacturer from, and without one the order
    // item is silently excluded from Payable (제조사 청구) creation entirely.
    public int? DirectManufacturerCompanyId { get; set; }
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

    // Broader than ExcludeCompleted (which only ever meant the literal "Completed"
    // status): hides every stage past the factory's own work (InspectedRequested
    // onward - inspection, settlement, delivery, completion) regardless of which
    // Status/status-group filter is also selected. Kept as its own flag rather than
    // widening ExcludeCompleted, since other pages (e.g. logistics-approval) already
    // depend on ExcludeCompleted meaning only "Completed".
    public bool? ExcludePostInspected { get; set; }

    // Narrower than ExcludePostInspected: hides only Inspected (물류도착) onward, but keeps
    // InspectedRequested (제품출고, awaiting the 물류도착 action itself) visible. Used by
    // logistics-approval's default worklist view so an order drops off that page the moment
    // it arrives and becomes a settlement charge, without hiding the orders still awaiting
    // that very action. Same "default/broad view only" guard as ExcludePostInspected.
    public bool? ExcludeArrived { get; set; }

    public bool? IsAsOnly { get; set; }

    public bool? IsPayableSettled { get; set; }

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

    public string? Memo { get; set; }

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
