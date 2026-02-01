using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.HasIndex(o => o.CustomerId);

        builder.Property(o => o.CustomerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.PremiumAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.StartDate)
            .IsRequired();

        builder.Property(o => o.EndDate)
            .IsRequired();

        builder.Property(o => o.Status)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.PaymentReferenceId)
            .HasMaxLength(100);

        builder.HasOne(o => o.Policy)
            .WithMany()
            .HasForeignKey(o => o.PolicyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
