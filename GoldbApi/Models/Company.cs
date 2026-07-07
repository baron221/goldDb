using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoldbApi.Models;

[Description("업체 정보")]
public class Company : BaseModel
{

    [Description("업체 ID")]
    public int Id { get; set; }

    [Description("상호명")]
    public string Name { get; set; } = string.Empty;

    [Description("대표자명")]
    public string CEO { get; set; } = string.Empty;

    [Description("지역")]
    public string Region { get; set; } = string.Empty;

    [Description("사업자 등록번호")]
    public string BusinessNumber { get; set; } = string.Empty;

    [Description("법인번호")]
    public string? CorporateNumber { get; set; }

    [Description("사업자 등록증 파일")]
    public string? BusinessLicenseFileUrl { get; set; }

    [Description("업태")]
    public string? BusinessType { get; set; }

    [Description("종목")]
    public string? BusinessCategory { get; set; }

    [Description("전화번호")]
    public string? Phone { get; set; }

    [Description("기본 주소")]
    public string? AddressBase { get; set; }

    [Description("상세 주소")]
    public string? AddressDetail { get; set; }

    [Description("우편번호")]
    public string? ZipCode { get; set; }

    [Description("물류코드")]
    public string? LogisticsCode { get; set; }

    [Description("센터번호")]
    public string? CenterNumber { get; set; }

    [Description("직영 여부")]
    public bool IsDirectManagement { get; set; } = false;

    [Description("업체 구분")]
    [MaxLength(50)]

    public string Category { get; set; } = "RTL";

    [Description("물류센터 ID")]
    public int? LogisticsCompanyId { get; set; }

    public virtual Company? LogisticsCompany { get; set; }

    public virtual ICollection<Company> Retailers { get; set; } = new List<Company>();

    [Description("은행명")]
    [MaxLength(50)]

    public string? BankName { get; set; }

    [Description("계좌번호")]
    [MaxLength(100)]

    public string? BankAccount { get; set; }

    [Description("예금주명")]
    [MaxLength(50)]

    public string? AccountHolder { get; set; }
}
