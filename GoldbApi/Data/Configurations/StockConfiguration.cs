using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Product)
               .WithMany()
               .HasForeignKey(e => e.ProductId);
        builder.HasOne(e => e.ProductSet)
               .WithMany()
               .HasForeignKey(e => e.ProductSetId);
        builder.HasOne(e => e.ParentStock)
               .WithMany(e => e.Children)
               .HasForeignKey(e => e.ParentStockId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.ExhaustionOrderItem)
               .WithMany(oi => oi.ExhaustedStocks)
               .HasForeignKey(e => e.ExhaustionOrderItemId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
