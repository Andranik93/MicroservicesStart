using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Orders.Infrastructure.Persistence;

/// <summary>
/// Lets the "dotnet ef" CLI create a DbContext at design time (for migrations),
/// without needing Orders.Api wired up yet. Not used at application runtime.
/// </summary>
public sealed class OrdersDbContextFactory : IDesignTimeDbContextFactory<OrdersDbContext>
{
    public OrdersDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ORDERS_DB_CONNECTION")
            ?? "Host=localhost;Port=5433;Database=orders;Username=orders_user;Password=orders_dev_password";

        var optionsBuilder = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseNpgsql(connectionString);

        return new OrdersDbContext(optionsBuilder.Options);
    }
}
