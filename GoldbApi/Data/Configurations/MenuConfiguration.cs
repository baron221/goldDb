using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Parent)
               .WithMany(e => e.Children)
               .HasForeignKey(e => e.ParentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
