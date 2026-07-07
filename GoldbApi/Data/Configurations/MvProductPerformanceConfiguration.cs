using GoldbApi.Models.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class MvProductPerformanceConfiguration : IEntityTypeConfiguration<MvProductPerformance>
{

    public void Configure(EntityTypeBuilder<MvProductPerformance> builder)
    {
        builder.HasNoKey().ToView("mv_product_performance");
    }
}
