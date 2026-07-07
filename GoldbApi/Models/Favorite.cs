using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoldbApi.Models;

[Description("즐겨찾기 정보")]
public class Favorite : BaseModel
{

    [Key]
    [Description("ID")]

    public int Id { get; set; }

    [Description("사용자 ID")]
    [Required]

    public int UserId { get; set; }

    public User? User { get; set; }

    [Description("제품 ID")]
    public int? ProductId { get; set; }

    public Product? Product { get; set; }

    [Description("세트 제품 ID")]
    public int? ProductSetId { get; set; }

    public ProductSet? ProductSet { get; set; }
}
