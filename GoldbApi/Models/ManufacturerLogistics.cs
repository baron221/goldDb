using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoldbApi.Models;

[Description("생산업체-물류센터 매핑")]
public class ManufacturerLogistics : BaseModel
{
    [Key]
    [Description("ID")]
    public int Id { get; set; }

    [Description("생산업체 ID")]
    public int ManufacturerId { get; set; }

    [Description("물류센터 ID")]
    public int LogisticsId { get; set; }

    public Company? Manufacturer { get; set; }

    public Company? Logistics { get; set; }
}
