using GoldbApi.Models.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class MvDailyOrderTrendConfiguration : IEntityTypeConfiguration<MvDailyOrderTrend>
{

    public void Configure(EntityTypeBuilder<MvDailyOrderTrend> builder)
    {
        builder.HasNoKey().ToView("mv_daily_order_trends");
    }
}
