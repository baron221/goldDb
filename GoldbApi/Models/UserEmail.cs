using System.ComponentModel;

namespace GoldbApi.Models;

[Description("사용자 이메일 정보")]
public class UserEmail : BaseModel
{

    [Description("이메일 ID")]
    public int Id { get; set; }

    [Description("사용자 ID")]
    public int UserId { get; set; }

    [Description("이메일 주소")]
    public string Email { get; set; } = string.Empty;

    [Description("대표 여부")]
    public bool IsPrimary { get; set; } = false;

    public User? User { get; set; }
}
