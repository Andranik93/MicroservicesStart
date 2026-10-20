using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Emails.Infrastructure.Persistence;

/// <summary>
/// Lets the "dotnet ef" CLI create a DbContext at design time, without
/// needing Emails.Api wired up yet. Not used at application runtime.
/// </summary>
public sealed class EmailsDbContextFactory : IDesignTimeDbContextFactory<EmailsDbContext>
{
    public EmailsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("EMAILS_DB_CONNECTION")
            ?? "Host=localhost;Port=5435;Database=emails;Username=emails_user;Password=emails_dev_password";

        var optionsBuilder = new DbContextOptionsBuilder<EmailsDbContext>()
            .UseNpgsql(connectionString);

        return new EmailsDbContext(optionsBuilder.Options);
    }
}
