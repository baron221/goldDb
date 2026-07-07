using System;
using Microsoft.EntityFrameworkCore;

namespace GoldbApi.Models.Views;

[Keyless]
public class MvMonthlyOrderTrend
{

    public DateTime OrderMonth { get; set; }

    public int OrderCount { get; set; }

    public decimal TotalAmount { get; set; }
}
