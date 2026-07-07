using System.ComponentModel;

namespace GoldbApi.Models;

[Description("기본 베이스 모델")]
public class BaseModel
{

    [Description("생성자 ID")]
    public int? CreatedBy { get; set; }

    [Description("수정자 ID")]
    public int? UpdatedBy { get; set; }

    [Description("생성 일시")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Description("수정 일시")]
    public DateTime? UpdatedAt { get; set; }

    [Description("삭제 여부")]
    public bool IsDeleted { get; set; } = false;
}
