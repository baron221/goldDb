using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class MenuFavoriteConfiguration : IEntityTypeConfiguration<MenuFavorite>
{
    public void Configure(EntityTypeBuilder<MenuFavorite> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.User)
               .WithMany()
               .HasForeignKey(e => e.UserId);
        builder.HasOne(e => e.Menu)
               .WithMany()
               .HasForeignKey(e => e.MenuId);
    }
}
