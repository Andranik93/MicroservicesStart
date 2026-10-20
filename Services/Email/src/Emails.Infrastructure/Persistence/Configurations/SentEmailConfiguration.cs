using Emails.Domain.EmailAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Emails.Infrastructure.Persistence.Configurations;

public sealed class SentEmailConfiguration : IEntityTypeConfiguration<SentEmail>
{
    public void Configure(EntityTypeBuilder<SentEmail> builder)
    {
        builder.ToTable("SentEmails");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.OrderId).IsRequired();

        builder.Property(e => e.Kind)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.ToAddress).HasMaxLength(320).IsRequired();
        builder.Property(e => e.Subject).HasMaxLength(200).IsRequired();
        builder.Property(e => e.SentAtUtc).IsRequired();

        // One email of a given kind per order - the idempotency guarantee,
        // enforced at the database level too, same pattern as Payment's
        // unique OrderId index.
        builder.HasIndex(e => new { e.OrderId, e.Kind }).IsUnique();
    }
}
