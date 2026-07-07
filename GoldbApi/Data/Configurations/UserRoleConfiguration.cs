using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {

        builder.HasKey(e => new { e.UserId, e.RoleId });

        builder.HasOne(e => e.User)
               .WithMany(e => e.UserRoles)
               .HasForeignKey(e => e.UserId);

        builder.HasOne(e => e.Role)
               .WithMany(e => e.UserRoles)
               .HasForeignKey(e => e.RoleId);
    }
}
