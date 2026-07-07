using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GoldbApi.Models;

namespace GoldbApi.Data.Configurations;

public class ManufacturerLogisticsConfiguration : IEntityTypeConfiguration<ManufacturerLogistics>
{
    public void Configure(EntityTypeBuilder<ManufacturerLogistics> builder)
    {
        builder.HasOne(ml => ml.Manufacturer)
               .WithMany()
               .HasForeignKey(ml => ml.ManufacturerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ml => ml.Logistics)
               .WithMany()
               .HasForeignKey(ml => ml.LogisticsId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
