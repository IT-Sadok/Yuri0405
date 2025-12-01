using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Configurations;

public class PaymentConfigurations : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> entity)
    {
        //Primary Key
        entity.HasKey(e => e.Id);

        // Column configurations
        entity.Property(e => e.IdempotencyKey)
            .IsRequired()
            .HasMaxLength(255);

        entity.Property(e => e.Amount)
            .HasPrecision(19, 4);

        entity.Property(e => e.Currency)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(3);

        entity.Property(e => e.ProviderPaymentId)
            .HasMaxLength(255);

        // Enum to int conversion
        entity.Property(e => e.Status)
            .HasConversion<int>();

        // Timestamps
        entity.Property(e => e.CreatedAt)
            .IsRequired();

        entity.Property(e => e.UpdatedAt)
            .IsRequired();
    }
}
