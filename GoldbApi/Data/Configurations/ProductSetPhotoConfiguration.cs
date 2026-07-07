using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class ProductSetPhotoConfiguration : IEntityTypeConfiguration<ProductSetPhoto>
{
    public void Configure(EntityTypeBuilder<ProductSetPhoto> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.ProductSet)
               .WithMany(s => s.ProductSetPhotos)
               .HasForeignKey(e => e.ProductSetId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Attachment>()
               .WithMany()
               .HasForeignKey(e => e.AttachmentId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
