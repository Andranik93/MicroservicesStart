using Microsoft.EntityFrameworkCore;
using Payments.Domain.PaymentAggregate;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Payments.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.OrderId).IsRequired();

        // One payment per order - enforced at the database level too, not just
        // in CreatePaymentHandler, so the rule holds even if another process
        // ever writes to this table directly.
        builder.HasIndex(p => p.OrderId).IsUnique();

        // Money here is a single property, not a collection item like Order's
        // OrderItem.UnitPrice was - so it can use ComplexProperty directly,
        // instead of needing OwnsOne nested inside an owned collection.
        builder.ComplexProperty(p => p.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Amount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.FailureReason).HasMaxLength(500);
        builder.Property(p => p.CreatedAtUtc).IsRequired();
    }
}
