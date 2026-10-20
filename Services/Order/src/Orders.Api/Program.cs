using Orders.Api.Consumers;
using Orders.Api.Endpoints;
using Orders.Api.ExceptionHandling;
using Orders.Application;
using Orders.Infrastructure;
using Orders.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrdersDbContext>();

builder.Services.AddOpenApi();

// Reuses the ConnectionFactory singleton AddInfrastructure already
// registered for the publisher - no separate RabbitMQ wiring needed here.
builder.Services.AddHostedService<PaymentOutcomeConsumer>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapHealthChecks("/health");
app.MapOrderEndpoints();

app.Run();
