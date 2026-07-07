using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoldbApi.Models;

[Description("사용자 개인 설정")]
public class UserPersonalSetting : BaseModel
{
    [Key]

    public int Id { get; set; }

    [Required]
    [Description("사용자 ID")]

    public int UserId { get; set; }

    public User? User { get; set; }

    [Required]
    [MaxLength(20)]
    [Description("글자 크기")]

    public string Size { get; set; } = "s12";

    [Required]
    [Description("Tags-View 사용 여부")]

    public bool TagsView { get; set; } = true;

    [Required]
    [Description("Header 고정 여부")]

    public bool FixedHeader { get; set; } = true;

    [Required]
    [Description("사이드바 로고 보기 여부")]

    public bool SidebarLogo { get; set; } = true;

    [Required]
    [Description("사이드바 하위 메뉴 팝업 여부")]

    public bool SecondMenuPopup { get; set; } = false;
}
