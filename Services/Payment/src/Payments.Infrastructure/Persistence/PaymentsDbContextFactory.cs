using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Payments.Infrastructure.Persistence;

/// <summary>
/// Lets the "dotnet ef" CLI create a DbContext at design time (for migrations),
/// without needing Payments.Api wired up yet. Not used at application runtime.
/// </summary>
public sealed class PaymentsDbContextFactory : IDesignTimeDbContextFactory<PaymentsDbContext>
{
    public PaymentsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("PAYMENTS_DB_CONNECTION")
            ?? "Host=localhost;Port=5434;Database=payments;Username=payments_user;Password=payments_dev_password";

        var optionsBuilder = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseNpgsql(connectionString);

        return new PaymentsDbContext(optionsBuilder.Options);
    }
}
