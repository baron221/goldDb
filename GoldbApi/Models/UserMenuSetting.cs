using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoldbApi.Models;

[Description("사용자별 메뉴 설정")]
public class UserMenuSetting : BaseModel
{
    [Key]

    public int Id { get; set; }

    [Required]

    public int UserId { get; set; }

    public User? User { get; set; }

    [Required]

    public int MenuId { get; set; }

    public Menu? Menu { get; set; }

    public bool? Affix { get; set; }

    public int SortOrder { get; set; } = 0;
}
