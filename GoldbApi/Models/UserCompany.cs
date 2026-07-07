using System.ComponentModel;

namespace GoldbApi.Models;

[Description("사용자-업체 연결")]
public class UserCompany : BaseModel
{

    [Description("사용자 ID")]
    public int UserId { get; set; }

    [Description("업체 ID")]
    public int CompanyId { get; set; }

    public User? User { get; set; }

    public Company? Company { get; set; }
}
