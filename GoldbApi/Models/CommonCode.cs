using System.ComponentModel;

namespace GoldbApi.Models;

[Description("공통 코드 정보")]
public class CommonCode : BaseModel
{

    [Description("코드 ID")]
    public int Id { get; set; }

    [Description("상위 코드 ID")]
    public int? ParentId { get; set; }

    [Description("코드값")]
    public string Code { get; set; } = string.Empty;

    [Description("코드명")]
    public string Name { get; set; } = string.Empty;

    [Description("코드 설명")]
    public string? Description { get; set; }

    [Description("정렬 순서")]
    public int SortOrder { get; set; } = 0;

    [Description("활성화 여부")]
    public bool IsActive { get; set; } = true;

    [Description("커스텀 필드 1")]
    public string? Custom1 { get; set; }

    [Description("커스텀 필드 2")]
    public string? Custom2 { get; set; }

    [Description("커스텀 필드 3")]
    public string? Custom3 { get; set; }

    [Description("커스텀 필드 4")]
    public string? Custom4 { get; set; }

    public CommonCode? Parent { get; set; }

    public List<CommonCode> Children { get; set; } = new();
}
