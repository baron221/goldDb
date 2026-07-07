using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class ProductOptionWeightConfiguration : IEntityTypeConfiguration<ProductOptionWeight>
{
    public void Configure(EntityTypeBuilder<ProductOptionWeight> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Product)
               .WithMany(p => p.OptionWeights)
               .HasForeignKey(e => e.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
