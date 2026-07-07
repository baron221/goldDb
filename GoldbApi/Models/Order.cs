using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoldbApi.Models;

[Description("주문 정보")]
public class Order : BaseModel
{

    [Key]
    [Description("ID")]

    public int Id { get; set; }

    [Description("주문번호")]
    [Required]
    [MaxLength(50)]

    public string OrderNo { get; set; } = string.Empty;

    [Description("그룹주문번호")]
    [MaxLength(50)]

    public string? GroupOrderNo { get; set; }

    [Description("사용자 ID")]
    public int UserId { get; set; }

    public User? User { get; set; }

    [Description("고객 ID")]
    public int? CustomerId { get; set; }

    public Customer? Customer { get; set; }

    [Description("총 주문 금액")]
    public decimal TotalAmount { get; set; }

    [Description("주문 상태")]
    [MaxLength(50)]

    public string Status { get; set; } = "ORDERED";

    [Description("주문 메모")]
    [MaxLength(500)]

    public string? OrderMemo { get; set; }

    [Description("공장 추가 메시지")]
    [MaxLength(500)]

    public string? FactoryRemarks { get; set; }

    [Description("물류 추가 메시지")]
    [MaxLength(500)]

    public string? LogisticsRemarks { get; set; }

    [Description("검수 요청 메시지")]
    [MaxLength(500)]

    public string? InspectionRemarks { get; set; }

    [Description("작업서 작성 메시지")]
    [MaxLength(500)]

    public string? WorkOrderRemarks { get; set; }

    [Description("물류업체 ID")]
    public int? LogisticsCompanyId { get; set; }

    public Company? LogisticsCompany { get; set; }

    [Description("취소 사유")]
    [MaxLength(500)]

    public string? CancellationReason { get; set; }

    [Description("정산 방법")]
    [MaxLength(50)]

    public string? SettlementMethod { get; set; }

    [Description("정산확인요청 메모")]
    [MaxLength(500)]

    public string? SettlementRemarks { get; set; }

    [Description("정산시작 메모")]
    [MaxLength(500)]
    public string? SettlementStartMemo { get; set; }

    [Description("정산 금액")]
    public decimal? SettlementAmount { get; set; }

    [Description("수정가공 의뢰 메모")]
    [MaxLength(500)]

    public string? ModificationMemo { get; set; }

    [Description("검수불가 종결 메모")]
    [MaxLength(500)]

    public string? CloseMemo { get; set; }

    [Description("납기일")]
    public DateTime? DeliveryDate { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public ICollection<OrderStatusHistory> StatusHistories { get; set; } = new List<OrderStatusHistory>();
}

[Description("주문 상세 정보")]
public class OrderItem : BaseModel
{

    [Key]
    [Description("ID")]

    public int Id { get; set; }

    [Description("주문 ID")]
    public int OrderId { get; set; }

    public Order? Order { get; set; }

    [Description("제품 ID")]
    public int? ProductId { get; set; }

    public Product? Product { get; set; }

    [Description("세트 제품 ID")]
    public int? ProductSetId { get; set; }

    public ProductSet? ProductSet { get; set; }

    [Description("상위 주문 상세 ID")]
    public int? ParentId { get; set; }

    public OrderItem? Parent { get; set; }

    public ICollection<OrderItem> Children { get; set; } = new List<OrderItem>();

    [Description("수량")]
    public int Quantity { get; set; }

    [Description("가격")]
    public decimal Price { get; set; }

    [Description("공장도 가격")]
    public decimal FactoryPrice { get; set; }

    [Description("수공비")]
    public decimal LaborCost { get; set; }

    [Description("공장 입력 재료비")]
    public decimal? FactoryInputMaterialCost { get; set; }

    [Description("공장 입력 수공비")]
    public decimal? FactoryInputLaborCost { get; set; }

    [Description("소매점 확인용 재료비")]
    public decimal? RetailerConfirmMaterialCost { get; set; }

    [Description("소매점 확인용 수공비")]
    public decimal? RetailerConfirmLaborCost { get; set; }

    [Description("제작생산일")]
    public DateTime? ProductionDate { get; set; }

    [Description("주문 당시 중량")]
    public decimal? OrderWeight { get; set; }

    [Description("실중량")]
    public decimal? ActualWeight { get; set; }

    [Description("검수 메모")]
    [MaxLength(500)]

    public string? InspectionMemo { get; set; }

    [Description("항량순도")]
    [MaxLength(20)]

    public string? Purity { get; set; }

    [Description("컬러")]
    [MaxLength(50)]

    public string? Color { get; set; }

    [Description("AS 주문 여부")]
    public bool IsAsOrder { get; set; }

    [Description("물류 확인 중량")]
    public decimal? ConfirmedWeight { get; set; }

    [Description("물류 검수 메모")]
    [MaxLength(500)]

    public string? LogisticsMemo { get; set; }

    [Description("물류 승인 중량")]
    public decimal? ApprovedWeight { get; set; }

    [Description("물류 승인 메모")]
    [MaxLength(500)]

    public string? ApprovedMemo { get; set; }

    [Description("공장 의뢰 중량")]
    public decimal? RequestedWeight { get; set; }

    [Description("공장 의뢰 메모")]
    [MaxLength(500)]

    public string? RequestedMemo { get; set; }

    [Description("정산 비율")]
    public decimal SettlementRatio { get; set; } = 70;

    [Description("정산 금액")]
    public decimal? SettlementAmount { get; set; }

    [Description("정산 메모")]
    [MaxLength(500)]

    public string? SettlementMemo { get; set; }

    public ICollection<Stock> ExhaustedStocks { get; set; } = new List<Stock>();
}
