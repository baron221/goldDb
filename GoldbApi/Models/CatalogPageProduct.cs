using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoldbApi.Models;

[Description("카탈로그 페이지 제품 연결 정보")]
public class CatalogPageProduct : BaseModel
{

    [Description("카탈로그 페이지 ID")]
    public int CatalogPageId { get; set; }

    public CatalogPage? CatalogPage { get; set; }

    [Description("제품 ID")]
    public int ProductId { get; set; }

    public Product? Product { get; set; }
}
