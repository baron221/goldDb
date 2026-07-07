using System.ComponentModel;

namespace GoldbApi.Models;

[Description("공지사항 정보")]
public class Notice : BaseModel
{

    [Description("공지사항 ID")]
    public int Id { get; set; }

    [Description("공지 제목")]
    public string Title { get; set; } = string.Empty;

    [Description("공지 내용")]
    public string Content { get; set; } = string.Empty;

    [Description("로그인 필요 여부")]
    public bool IsLoginRequired { get; set; } = false;

    [Description("활성화 여부")]
    public bool IsActive { get; set; } = true;

    [Description("조회수")]
    public int ViewCount { get; set; } = 0;

    [Description("노출 시작일")]
    public DateTime? StartDate { get; set; }

    [Description("노출 종료일")]
    public DateTime? EndDate { get; set; }
}
