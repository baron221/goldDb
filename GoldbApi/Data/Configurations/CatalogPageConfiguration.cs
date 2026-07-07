using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class CatalogPageConfiguration : IEntityTypeConfiguration<CatalogPage>
{
    public void Configure(EntityTypeBuilder<CatalogPage> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Catalog)
               .WithMany(c => c.CatalogPages)
               .HasForeignKey(e => e.CatalogId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Company)
               .WithMany()
               .HasForeignKey(e => e.CompanyId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
