using System.ComponentModel;

namespace GoldbApi.Models;

[Description("사용자 사진 정보")]
public class UserPhoto : BaseModel
{

    [Description("사진 ID")]
    public int Id { get; set; }

    [Description("사용자 ID")]
    public int UserId { get; set; }

    [Description("사진 URL")]
    public string PhotoUrl { get; set; } = string.Empty;

    [Description("첨부파일 ID")]
    public int? AttachmentId { get; set; }

    [Description("정렬 순서")]
    public int SortOrder { get; set; } = 0;

    public User? User { get; set; }
}
