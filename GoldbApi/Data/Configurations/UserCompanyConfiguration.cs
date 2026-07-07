using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class UserCompanyConfiguration : IEntityTypeConfiguration<UserCompany>
{
    public void Configure(EntityTypeBuilder<UserCompany> builder)
    {
        builder.HasKey(e => new { e.UserId, e.CompanyId });

        builder.HasOne(e => e.User)
               .WithMany(e => e.UserCompanies)
               .HasForeignKey(e => e.UserId);

        builder.HasOne(e => e.Company)
               .WithMany()
               .HasForeignKey(e => e.CompanyId);
    }
}
