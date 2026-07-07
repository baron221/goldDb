using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Company)
               .WithMany()
               .HasForeignKey(e => e.CompanyId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
