using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoldbApi.Models;

[Description("재고 정보")]
public class Stock : BaseModel
{

    [Key]
    [Description("ID")]

    public int Id { get; set; }

    [Description("제품 ID")]
    public int? ProductId { get; set; }

    public Product? Product { get; set; }

    [Description("세트 제품 ID")]
    public int? ProductSetId { get; set; }

    public ProductSet? ProductSet { get; set; }

    [Description("상위 재고 ID")]
    public int? ParentStockId { get; set; }

    public Stock? ParentStock { get; set; }

    public ICollection<Stock> Children { get; set; } = new List<Stock>();

    [Description("소속 업체 ID")]
    public int? CompanyId { get; set; }

    public Company? Company { get; set; }

    [Description("재고번호")]
    [MaxLength(100)]

    public string StockNo { get; set; } = string.Empty;

    [Description("상태")]
    [MaxLength(50)]

    public string Status { get; set; } = "IN_STOCK";

    [Description("함량")]
    [MaxLength(20)]

    public string? Purity { get; set; }

    [Description("컬러")]
    [MaxLength(50)]

    public string? Color { get; set; }

    [Description("실중량")]
    public decimal ActualWeight { get; set; }

    [Description("제작생산일")]
    public DateTime? ProductionDate { get; set; }

    [Description("대여자명")]
    [MaxLength(100)]

    public string? RenterName { get; set; }

    [Description("대여일")]
    public DateTime? RentDate { get; set; }

    [Description("반납예정일")]
    public DateTime? ReturnDueDate { get; set; }

    [Description("비고")]
    [MaxLength(500)]

    public string? Note { get; set; }

    [Description("원천 주문 ID")]
    public int? SourceOrderId { get; set; }

    public Order? SourceOrder { get; set; }

    [Description("원천 주문 상세 ID")]
    public int? SourceOrderItemId { get; set; }

    public OrderItem? SourceOrderItem { get; set; }

    [Description("소매점 확인용 재료비")]
    public decimal? RetailerConfirmMaterialCost { get; set; }

    [Description("소매점 확인용 수공비")]
    public decimal? RetailerConfirmLaborCost { get; set; }

    [Description("소진 여부")]
    public bool IsExhausted { get; set; } = false;

    [Description("소진일")]
    public DateTime? ExhaustionDate { get; set; }

    [Description("소진 주문 ID")]
    public int? ExhaustionOrderId { get; set; }

    public Order? ExhaustionOrder { get; set; }

    [Description("소진 주문 상세 ID")]
    public int? ExhaustionOrderItemId { get; set; }

    public OrderItem? ExhaustionOrderItem { get; set; }

    public List<Attachment> Attachments { get; set; } = new();
}
