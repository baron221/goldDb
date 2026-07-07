using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class ProductSetConfiguration : IEntityTypeConfiguration<ProductSet>
{
    public void Configure(EntityTypeBuilder<ProductSet> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Company)
               .WithMany()
               .HasForeignKey(e => e.CompanyId);
    }
}
