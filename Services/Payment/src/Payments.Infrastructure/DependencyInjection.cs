using RabbitMQ.Client;
using Microsoft.EntityFrameworkCore;
using Payments.Infrastructure.Messaging;
using Payments.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Payments.Infrastructure.Persistence;
using Payments.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Payments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PaymentsDb")
            ?? throw new InvalidOperationException("Connection string 'PaymentsDb' is not configured.");

        services.AddDbContext<PaymentsDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // RabbitMq configuration also used by Payments.Api's OrderConfirmedConsumer -
        // registered here now that Payment publishes events too, matching Order's
        // existing pattern (Program.cs no longer needs its own copy of this).
        var rabbitMq = configuration.GetSection("RabbitMq");
        var connectionFactory = new ConnectionFactory
        {
            HostName = rabbitMq["HostName"] ?? throw new InvalidOperationException("RabbitMq:HostName is not configured."),
            Port = int.Parse(rabbitMq["Port"] ?? "5672"),
            UserName = rabbitMq["UserName"] ?? throw new InvalidOperationException("RabbitMq:UserName is not configured."),
            Password = rabbitMq["Password"] ?? throw new InvalidOperationException("RabbitMq:Password is not configured.")
        };

        services.AddSingleton(connectionFactory);
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

        return services;
    }
}
