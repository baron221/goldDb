using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class UserPersonalSettingConfiguration : IEntityTypeConfiguration<UserPersonalSetting>
{
    public void Configure(EntityTypeBuilder<UserPersonalSetting> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.User)
               .WithMany()
               .HasForeignKey(e => e.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
