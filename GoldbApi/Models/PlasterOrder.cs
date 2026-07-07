using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoldbApi.Models;

[Description("석고 발주 정보")]
public class PlasterOrder : BaseModel
{

    [Key]
    [Description("ID")]

    public int Id { get; set; }

    [Description("발주사 ID")]
    public int? OrderingCompanyId { get; set; }

    public Company? OrderingCompany { get; set; }

    [Description("발주 제조사 ID")]
    public int? ManufacturerId { get; set; }

    public Company? Manufacturer { get; set; }

    [Description("수량")]
    public int Quantity { get; set; }

    [Description("상태")]
    [MaxLength(20)]

    public string Status { get; set; } = "발주";

    [Description("발주일자")]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [Description("취소 여부")]
    public bool IsCancelled { get; set; } = false;

    [Description("비고")]
    [MaxLength(500)]

    public string Remarks { get; set; } = string.Empty;
}
