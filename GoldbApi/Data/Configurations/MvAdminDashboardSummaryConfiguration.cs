using GoldbApi.Models.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class MvAdminDashboardSummaryConfiguration : IEntityTypeConfiguration<MvAdminDashboardSummary>
{

    public void Configure(EntityTypeBuilder<MvAdminDashboardSummary> builder)
    {
        builder.HasNoKey().ToView("mv_admin_dashboard_summary");
    }
}
