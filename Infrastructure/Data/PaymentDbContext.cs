using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Application.DTOs;

namespace Infrastructure.Data;

public class PaymentDbContext: DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Payment> Payments { get; set; }
    public DbSet<OutboxPaymentMessage> OutboxPaymentMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
    }
}
