using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldbApi.Data.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Order)
               .WithMany(o => o.StatusHistories)
               .HasForeignKey(e => e.OrderId);
        builder.HasOne(e => e.User)
               .WithMany()
               .HasForeignKey(e => e.UserId);
    }
}
