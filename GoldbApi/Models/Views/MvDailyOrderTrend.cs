using System;
using Microsoft.EntityFrameworkCore;

namespace GoldbApi.Models.Views;

[Keyless]
public class MvDailyOrderTrend
{

    public DateTime OrderDate { get; set; }

    public int OrderCount { get; set; }

    public decimal TotalAmount { get; set; }
}
