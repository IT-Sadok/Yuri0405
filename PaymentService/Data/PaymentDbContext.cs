using Microsoft.EntityFrameworkCore;
using PaymentService.Models.Entities;

namespace PaymentService.Data;

public class PaymentDbContext: DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) 
        : base(options)
    {
    }
    
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Payment>(entity =>
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

        });
    }
}