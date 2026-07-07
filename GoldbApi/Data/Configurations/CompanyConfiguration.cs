using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.LogisticsCompany)
               .WithMany(c => c.Retailers)
               .HasForeignKey(e => e.LogisticsCompanyId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
