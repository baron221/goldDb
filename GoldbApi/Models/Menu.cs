using System.ComponentModel;

namespace GoldbApi.Models;

[Description("메뉴 정보")]
public class Menu : BaseModel
{

    [Description("메뉴 ID")]
    public int Id { get; set; }

    [Description("상위 메뉴 ID")]
    public int? ParentId { get; set; }

    [Description("메뉴 경로")]
    public string Path { get; set; } = string.Empty;

    [Description("컴포넌트 경로")]
    public string? Component { get; set; }

    [Description("메뉴 이름")]
    public string? Name { get; set; }

    [Description("리다이렉트 경로")]
    public string? Redirect { get; set; }

    [Description("메뉴 제목")]
    public string? Title { get; set; }

    [Description("아이콘")]
    public string? Icon { get; set; }

    [Description("캐시 사용 여부")]
    public bool? NoCache { get; set; }

    [Description("태그바 고정 여부")]
    public bool? Affix { get; set; }

    [Description("숨김 여부")]
    public bool? Hidden { get; set; }

    [Description("항상 표시 여부")]
    public bool? AlwaysShow { get; set; }

    [Description("활성 메뉴")]
    public string? ActiveMenu { get; set; }

    [Description("중복화면 허용 여부")]
    public bool? AllowDuplicate { get; set; }

    [Description("모바일 여부")]
    public bool IsMobile { get; set; } = false;

    [Description("정렬 순서")]
    public int SortOrder { get; set; } = 0;

    public Menu? Parent { get; set; }

    public List<Menu> Children { get; set; } = new();
}
