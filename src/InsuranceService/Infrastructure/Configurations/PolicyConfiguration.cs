using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PolicyNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.PolicyNumber)
            .IsUnique();

        builder.Property(p => p.CustomerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.PremiumAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.StartDate)
            .IsRequired();

        builder.Property(p => p.EndDate)
            .IsRequired();

        builder.Property(p => p.Status)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.PaymentReferenceId)
            .HasMaxLength(100);
    }
}
