using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class ProductSetItemConfiguration : IEntityTypeConfiguration<ProductSetItem>
{
    public void Configure(EntityTypeBuilder<ProductSetItem> builder)
    {
        builder.HasKey(e => new { e.ProductSetId, e.ProductId });
        builder.HasOne(e => e.ProductSet)
               .WithMany(s => s.SetItems)
               .HasForeignKey(e => e.ProductSetId);
        builder.HasOne(e => e.Product)
               .WithMany()
               .HasForeignKey(e => e.ProductId);
    }
}
