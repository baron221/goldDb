using System.ComponentModel;

namespace GoldbApi.Models;

[Description("사용자 전화번호 정보")]
public class UserPhone : BaseModel
{

    [Description("전화번호 ID")]
    public int Id { get; set; }

    [Description("사용자 ID")]
    public int UserId { get; set; }

    [Description("전화번호")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Description("대표 여부")]
    public bool IsPrimary { get; set; } = false;

    public User? User { get; set; }
}
