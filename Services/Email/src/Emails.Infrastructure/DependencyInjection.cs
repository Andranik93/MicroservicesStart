using Emails.Application.Abstractions;
using Emails.Infrastructure.Email;
using Emails.Infrastructure.ExternalServices;
using Emails.Infrastructure.Persistence;
using Emails.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Emails.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("EmailsDb")
            ?? throw new InvalidOperationException("Connection string 'EmailsDb' is not configured.");

        services.AddDbContext<EmailsDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IEmailRepository, EmailRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // SmtpEmailSender reads Smtp:Host/Port itself via IConfiguration - no
        // per-call setup needed here beyond registering it.
        services.AddSingleton<IEmailSender, SmtpEmailSender>();

        var orderApiBaseUrl = configuration["Services:OrderApi"]
            ?? throw new InvalidOperationException("Services:OrderApi is not configured.");

        services.AddHttpClient<IOrderServiceClient, OrderServiceClient>(client =>
        {
            client.BaseAddress = new Uri(orderApiBaseUrl);
        });

        return services;
    }
}
