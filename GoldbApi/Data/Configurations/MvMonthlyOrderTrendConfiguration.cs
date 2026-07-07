using GoldbApi.Models.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class MvMonthlyOrderTrendConfiguration : IEntityTypeConfiguration<MvMonthlyOrderTrend>
{

    public void Configure(EntityTypeBuilder<MvMonthlyOrderTrend> builder)
    {
        builder.HasNoKey().ToView("mv_monthly_order_trends");
    }
}
