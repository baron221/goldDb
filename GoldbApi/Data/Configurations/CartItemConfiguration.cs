using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.User)
               .WithMany()
               .HasForeignKey(e => e.UserId);
        builder.HasOne(e => e.Product)
               .WithMany()
               .HasForeignKey(e => e.ProductId);
        builder.HasOne(e => e.ProductSet)
               .WithMany()
               .HasForeignKey(e => e.ProductSetId);
    }
}
