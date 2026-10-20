using Payments.Api.Consumers;
using Payments.Api.Endpoints;
using Payments.Api.ExceptionHandling;
using Payments.Api.Simulators;
using Payments.Application;
using Payments.Infrastructure;
using Payments.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<PaymentsDbContext>();

builder.Services.AddOpenApi();

// Reuses the ConnectionFactory singleton AddInfrastructure now registers
// (Payment publishes events too, as of this step) - no separate wiring here.
builder.Services.AddHostedService<OrderConfirmedConsumer>();

// Stand-in for a real payment gateway - see FakePaymentGatewaySimulator for why.
// Toggle off once a real integration exists.
if (builder.Configuration.GetValue("FakePaymentGateway:Enabled", true))
{
    builder.Services.AddHostedService<FakePaymentGatewaySimulator>();
}

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapHealthChecks("/health");
app.MapPaymentEndpoints();

app.Run();
