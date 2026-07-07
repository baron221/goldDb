using Microsoft.EntityFrameworkCore;

namespace GoldbApi.Models.Views;

[Keyless]
public class MvProductPerformance
{

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string ProductNo { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal TotalAmount { get; set; }
}
