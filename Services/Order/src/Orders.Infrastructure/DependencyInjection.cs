using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Application.Abstractions;
using Orders.Infrastructure.Messaging;
using Orders.Infrastructure.Persistence;
using Orders.Infrastructure.Repositories;
using RabbitMQ.Client;

namespace Orders.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrdersDb")
            ?? throw new InvalidOperationException("Connection string 'OrdersDb' is not configured.");

        services.AddDbContext<OrdersDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

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
