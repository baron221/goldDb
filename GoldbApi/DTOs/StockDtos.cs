namespace GoldbApi.DTOs;

public class StockDto
{

    public int Id { get; set; }

    public int? ProductId { get; set; }

    public int? ProductSetId { get; set; }

    public int? ParentStockId { get; set; }

    public int? CompanyId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string ProductSetTitle { get; set; } = string.Empty;

    public string ProductNo { get; set; } = string.Empty;

    public string? ProductPhotoUrl { get; set; }

    public string? CategoryLarge { get; set; }

    public string? CategoryMedium { get; set; }

    public string? CategorySmall { get; set; }

    public string Category { get; set; } = string.Empty;

    public string StockNo { get; set; } = string.Empty;

    public string Status { get; set; } = "IN_STOCK";

    public string Purity { get; set; } = string.Empty;

    public string? Color { get; set; }

    public string? Size { get; set; }

    public int Quantity { get; set; } = 1;

    public decimal ActualWeight { get; set; }

    public DateTime? ProductionDate { get; set; }

    public string? CompanyName { get; set; }

    public string? OwnerCompanyName { get; set; }

    public string? LogisticsCompanyName { get; set; }

    public string? SourceOrderNo { get; set; }

    public DateTime? SourceOrderDate { get; set; }

    public string? CustomerName { get; set; }

    public string? RenterName { get; set; }

    public DateTime? RentDate { get; set; }

    public DateTime? ReturnDueDate { get; set; }

    public decimal? RetailerConfirmMaterialCost { get; set; }

    public decimal? RetailerConfirmLaborCost { get; set; }

    public string? Note { get; set; }

    public int? SourceOrderId { get; set; }

    public int? SourceOrderItemId { get; set; }

    public int? ExhaustionOrderId { get; set; }

    public string? ExhaustionOrderNo { get; set; }

    public int? ExhaustionOrderItemId { get; set; }

    public bool IsExhausted { get; set; }

    public DateTime? ExhaustionDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<AttachmentDto> Attachments { get; set; } = new();

    public List<StockDto> Children { get; set; } = new();
}

public class AttachmentDto
{

    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string OriginalName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public bool IsMain { get; set; }
}

public class StockDeleteDto
{

    public List<int> Ids { get; set; } = new();

    public string Reason { get; set; } = string.Empty;

    public string? Memo { get; set; }

    public int? ExhaustionOrderId { get; set; }

    public int? ExhaustionOrderItemId { get; set; }

    public DateTime? ExhaustionDate { get; set; }
}

public class StockPhotoUpdateDto
{

    public List<AttachmentUpdateDto> Attachments { get; set; } = new();

    public int? MainAttachmentId { get; set; }
}

public class AttachmentUpdateDto
{

    public int AttachmentId { get; set; }
}

public class StockDetailDto : StockDto
{

    public ProductDto? Product { get; set; }

    public ProductSetDto? ProductSet { get; set; }

    public CompanyDto? Manufacturer { get; set; }

    public CompanyDto? LogisticsCompany { get; set; }

    public OrderDto? SourceOrder { get; set; }

    public List<OrderStatusHistoryDto> OrderHistory { get; set; } = new();
}

public class CreateStockDto
{

    public int? ProductId { get; set; }

    public int? ProductSetId { get; set; }

    public int? CompanyId { get; set; }

    public string StockNo { get; set; } = string.Empty;

    public string Status { get; set; } = "IN_STOCK";

    public string? Purity { get; set; }

    public string? Color { get; set; }

    public string? Size { get; set; }

    public int Quantity { get; set; } = 1;

    public decimal ActualWeight { get; set; }

    public string? Note { get; set; }

    public decimal? RetailerConfirmMaterialCost { get; set; }

    public decimal? RetailerConfirmLaborCost { get; set; }
}

public class UpdateStockDto
{

    public string? Status { get; set; }

    public decimal? ActualWeight { get; set; }

    public string? RenterName { get; set; }

    public DateTime? RentDate { get; set; }

    public DateTime? ReturnDueDate { get; set; }

    public string? Note { get; set; }

    public decimal? RetailerConfirmMaterialCost { get; set; }

    public decimal? RetailerConfirmLaborCost { get; set; }
}

public class StockSummaryDto
{

    public decimal Total14KWeight { get; set; }

    public decimal Total18KWeight { get; set; }

    public decimal TotalPureGoldWeight { get; set; }

    public decimal TotalCalculatedPureGoldWeight { get; set; }
}

public class StockQueryDto
{

    public int? CompanyId { get; set; }

    public int? LogisticsCompanyId { get; set; }

    public bool? IsDirectManagement { get; set; }

    public string? CategoryLarge { get; set; }

    public string? CategoryMedium { get; set; }

    public string? CategorySmall { get; set; }

    public string? SetCategoryLarge { get; set; }

    public string? SetCategoryMedium { get; set; }

    public string? SetCategorySmall { get; set; }

    public string? OrderNo { get; set; }

    public string? StockNo { get; set; }

    public string? ProductName { get; set; }

    public string? SearchText { get; set; }

    public decimal? MinWeight { get; set; }

    public decimal? MaxWeight { get; set; }

    public string? Status { get; set; }

    public bool? IsExhausted { get; set; }

    public DateTime? ExhaustionDateStart { get; set; }

    public DateTime? ExhaustionDateEnd { get; set; }

    public DateTime? CreatedAtStart { get; set; }

    public DateTime? CreatedAtEnd { get; set; }

    public int? Page { get; set; } = 1;

    public int? PageSize { get; set; } = 50;
}

public class StockRentDto
{

    public List<int> StockIds { get; set; } = new();

    public string RenterName { get; set; } = string.Empty;

    public DateTime? ReturnDueDate { get; set; }
}

public class StockBarcodeActionDto
{

    public string Barcode { get; set; } = string.Empty;

    public string RenterName { get; set; } = string.Empty;

    public DateTime? ReturnDueDate { get; set; }
}

public class ExhaustStockOrderItemDto
{

    public int OrderItemId { get; set; }

    public int StockId { get; set; }

    public string? Memo { get; set; }
}
