using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class PlasterOrderConfiguration : IEntityTypeConfiguration<PlasterOrder>
{
    public void Configure(EntityTypeBuilder<PlasterOrder> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.OrderingCompany)
               .WithMany()
               .HasForeignKey(e => e.OrderingCompanyId);
        builder.HasOne(e => e.Manufacturer)
               .WithMany()
               .HasForeignKey(e => e.ManufacturerId);
    }
}
