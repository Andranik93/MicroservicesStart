using Orders.Domain.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Orders.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the Order aggregate. OrderItem has no identity of its own, so it is
/// mapped as an owned collection: it only ever exists attached to an Order,
/// in its own "OrderItems" table, and EF Core loads it automatically with
/// the Order - no .Include(...) call needed.
/// </summary>
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerId).IsRequired();
        builder.Property(o => o.Currency).IsRequired().HasMaxLength(3);
        builder.Property(o => o.CreatedAtUtc).IsRequired();

        // Backs GetOrdersByCustomerIdQuery, which filters on this column.
        builder.HasIndex(o => o.CustomerId);

        // Stored as text ("Pending", "Confirmed", ...) instead of the numeric enum value,
        // so the data is readable with a plain SQL query while you're learning.
        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.PaymentFailureReason).HasMaxLength(500);

        // Total is computed from Items, not a column.
        builder.Ignore(o => o.Total);

        // Items is a read-only property with no setter, so EF Core must use the
        // private "_items" backing field directly - this line tells it to.
        builder.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(o => o.Items, items =>
        {
            items.ToTable("OrderItems");
            items.WithOwner().HasForeignKey("OrderId");

            // OrderItem has no Id in the domain model; EF Core needs one to track rows,
            // so this key exists only in the database, never in the domain.
            items.Property<Guid>("Id").ValueGeneratedOnAdd();
            items.HasKey("Id");

            items.Property(i => i.ProductId).IsRequired();
            items.Property(i => i.Quantity).IsRequired();

            // Money is itself an owned type nested inside OrderItem. By default EF Core
            // keeps a nested owned type in the same table, so this becomes two plain
            // columns on OrderItems rather than a separate table.
            items.OwnsOne(i => i.UnitPrice, price =>
            {
                price.Property(m => m.Amount).HasColumnName("UnitPriceAmount").HasPrecision(18, 2);
                price.Property(m => m.Currency).HasColumnName("UnitPriceCurrency").HasMaxLength(3);
            });
        });
    }
}
