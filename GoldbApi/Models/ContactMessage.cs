using System.ComponentModel;

namespace GoldbApi.Models;

[Description("고객 문의 메시지")]
public class ContactMessage : BaseModel
{

    public int Id { get; set; }

    [Description("이름")]

    public string Name { get; set; } = string.Empty;

    [Description("이메일")]

    public string Email { get; set; } = string.Empty;

    [Description("전화번호")]

    public string? Phone { get; set; }

    [Description("주제")]

    public string Subject { get; set; } = string.Empty;

    [Description("메시지 내용")]

    public string Message { get; set; } = string.Empty;

    [Description("처리 여부")]

    public bool IsProcessed { get; set; } = false;

    [Description("처리 일시")]

    public DateTime? ProcessedAt { get; set; }

    [Description("처리자 ID")]

    public int? ProcessedBy { get; set; }

    [Description("관리자 메모")]

    public string? AdminMemo { get; set; }
}
