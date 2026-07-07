using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class CatalogPageProductConfiguration : IEntityTypeConfiguration<CatalogPageProduct>
{
    public void Configure(EntityTypeBuilder<CatalogPageProduct> builder)
    {
        builder.HasKey(e => new { e.CatalogPageId, e.ProductId });
        builder.HasOne(e => e.CatalogPage)
               .WithMany(c => c.CatalogPageProducts)
               .HasForeignKey(e => e.CatalogPageId);
        builder.HasOne(e => e.Product)
               .WithMany()
               .HasForeignKey(e => e.ProductId);
    }
}
