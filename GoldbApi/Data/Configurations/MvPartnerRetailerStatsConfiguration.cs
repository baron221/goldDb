using GoldbApi.Models.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class MvPartnerRetailerStatsConfiguration : IEntityTypeConfiguration<MvPartnerRetailerStats>
{

    public void Configure(EntityTypeBuilder<MvPartnerRetailerStats> builder)
    {
        builder.HasNoKey().ToView("mv_partner_retailer_stats");
    }
}
