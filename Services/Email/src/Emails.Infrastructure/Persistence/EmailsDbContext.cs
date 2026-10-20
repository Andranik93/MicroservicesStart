using Emails.Domain.EmailAggregate;
using Microsoft.EntityFrameworkCore;

namespace Emails.Infrastructure.Persistence;

public sealed class EmailsDbContext(DbContextOptions<EmailsDbContext> options) : DbContext(options)
{
    public DbSet<SentEmail> SentEmails => Set<SentEmail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmailsDbContext).Assembly);
    }
}
