using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class MenuPermissionConfiguration : IEntityTypeConfiguration<MenuPermission>
{
    public void Configure(EntityTypeBuilder<MenuPermission> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Menu)
               .WithMany()
               .HasForeignKey(e => e.MenuId);
    }
}
