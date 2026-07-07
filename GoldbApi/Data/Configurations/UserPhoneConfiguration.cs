using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class UserPhoneConfiguration : IEntityTypeConfiguration<UserPhone>
{
    public void Configure(EntityTypeBuilder<UserPhone> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.User)
               .WithMany(e => e.UserPhones)
               .HasForeignKey(e => e.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
