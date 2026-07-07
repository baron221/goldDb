using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class ProductPhotoConfiguration : IEntityTypeConfiguration<ProductPhoto>
{
    public void Configure(EntityTypeBuilder<ProductPhoto> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Product)
               .WithMany(p => p.ProductPhotos)
               .HasForeignKey(e => e.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Attachment>()
               .WithMany()
               .HasForeignKey(e => e.AttachmentId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
