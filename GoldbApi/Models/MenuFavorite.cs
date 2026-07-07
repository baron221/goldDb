using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoldbApi.Models;

[Description("메뉴 즐겨찾기 정보")]
public class MenuFavorite : BaseModel
{

    [Key]
    [Description("ID")]

    public int Id { get; set; }

    [Description("사용자 ID")]
    [Required]

    public int UserId { get; set; }

    public User? User { get; set; }

    [Description("메뉴 ID")]
    [Required]

    public int MenuId { get; set; }

    public Menu? Menu { get; set; }

    [Description("정렬 순서")]
    public int SortOrder { get; set; } = 0;
}
