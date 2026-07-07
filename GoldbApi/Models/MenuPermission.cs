using System.ComponentModel;

namespace GoldbApi.Models;

[Description("메뉴 권한 정보")]
public class MenuPermission : BaseModel
{

    [Description("권한 ID")]
    public int Id { get; set; }

    [Description("메뉴 ID")]
    public int MenuId { get; set; }

    [Description("역할 키")]
    public string RoleKey { get; set; } = string.Empty;

    [Description("조회 권한")]
    public bool CanSearch { get; set; } = false;

    [Description("추가 권한")]
    public bool CanCreate { get; set; } = false;

    [Description("삭제 권한")]
    public bool CanDelete { get; set; } = false;

    [Description("저장 권한")]
    public bool CanSave { get; set; } = false;

    [Description("출력 권한")]
    public bool CanPrint { get; set; } = false;

    [Description("커스텀 권한 1")]
    public bool Custom1 { get; set; } = false;

    [Description("커스텀 권한 2")]
    public bool Custom2 { get; set; } = false;

    [Description("커스텀 권한 3")]
    public bool Custom3 { get; set; } = false;

    [Description("커스텀 권한 4")]
    public bool Custom4 { get; set; } = false;

    [Description("커스텀 권한 5")]
    public bool Custom5 { get; set; } = false;

    [Description("커스텀 권한 6")]
    public bool Custom6 { get; set; } = false;

    [Description("커스텀 권한 7")]
    public bool Custom7 { get; set; } = false;

    [Description("커스텀 권한 8")]
    public bool Custom8 { get; set; } = false;

    public Menu? Menu { get; set; }
}
