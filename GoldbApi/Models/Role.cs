using System.ComponentModel;

namespace GoldbApi.Models;

[Description("역할 정보")]
public class Role : BaseModel
{

    [Description("역할 ID")]
    public int Id { get; set; }

    [Description("역할 키")]
    public string Key { get; set; } = string.Empty;

    [Description("역할 이름")]
    public string Name { get; set; } = string.Empty;

    [Description("역할 설명")]
    public string? Description { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
