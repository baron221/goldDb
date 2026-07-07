using System.ComponentModel;

namespace GoldbApi.Models;

[Description("게시글 정보")]
public class Article : BaseModel
{

    [Description("게시글 ID")]
    public int Id { get; set; }

    [Description("타임스탬프")]
    public long Timestamp { get; set; }

    [Description("작성자")]
    public string Author { get; set; } = string.Empty;

    [Description("검토자")]
    public string? Reviewer { get; set; }

    [Description("제목")]
    public string Title { get; set; } = string.Empty;

    [Description("요약")]
    public string? ContentShort { get; set; }

    [Description("내용")]
    public string Content { get; set; } = string.Empty;

    [Description("예측 수치")]
    public double Forecast { get; set; }

    [Description("중요도")]
    public int Importance { get; set; }

    [Description("타입")]
    public string Type { get; set; } = "CN";

    [Description("상태")]
    public string Status { get; set; } = "draft";

    [Description("표시 시간")]
    public DateTime DisplayTime { get; set; } = DateTime.UtcNow;

    [Description("댓글 금지 여부")]
    public bool CommentDisabled { get; set; } = true;

    [Description("조회수")]
    public int Pageviews { get; set; }

    [Description("이미지 URI")]
    public string? ImageUri { get; set; }

    [Description("소스 URI")]
    public string? SourceUri { get; set; }

    [Description("플랫폼")]
    public string[] Platforms { get; set; } = Array.Empty<string>();
}
