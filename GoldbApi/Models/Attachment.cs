using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoldbApi.Models;

[Description("첨부 파일 및 미디어 정보")]
public class Attachment : BaseModel
{

    [Description("첨부파일 ID")]
    public int Id { get; set; }

    [Description("저장된 파일명")]
    public string FileName { get; set; } = string.Empty;

    [Description("원본 파일명")]
    public string OriginalName { get; set; } = string.Empty;

    [Description("파일 확장자")]
    public string Extension { get; set; } = string.Empty;

    [Description("MIME 타입")]
    public string MimeType { get; set; } = string.Empty;

    [Description("파일 크기")]
    public long FileSize { get; set; }

    [Description("저장 경로")]
    public string FilePath { get; set; } = string.Empty;

    [Description("하위 디렉토리")]
    public string? SubDirectory { get; set; }

    [Description("대표 사진 여부")]
    public bool IsMain { get; set; } = false;
}
