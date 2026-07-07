using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoldbApi.Models;

[Description("고객 정보")]
public class Customer : BaseModel
{

    [Description("고객 ID")]
    public int Id { get; set; }

    [Description("이름")]
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Description("연락처")]
    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Description("이메일")]
    [MaxLength(100)]
    public string? Email { get; set; }

    [Description("생년월일")]
    public DateTime? BirthDate { get; set; }

    [Description("비고")]
    [MaxLength(500)]
    public string? Note { get; set; }

    [Description("담당 업체 ID")]
    public int? CompanyId { get; set; }

    public virtual Company? Company { get; set; }
}
