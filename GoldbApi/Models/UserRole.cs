using System.ComponentModel;

namespace GoldbApi.Models;

[Description("사용자-역할 매핑")]
public class UserRole : BaseModel
{

    [Description("사용자 ID")]
    public int UserId { get; set; }

    [Description("역할 ID")]
    public int RoleId { get; set; }

    public User? User { get; set; }

    public Role? Role { get; set; }
}
