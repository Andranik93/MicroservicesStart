using Emails.Api.Consumers;
using Emails.Api.Endpoints;
using Emails.Api.ExceptionHandling;
using Emails.Application;
using Emails.Infrastructure;
using Emails.Infrastructure.Persistence;
using RabbitMQ.Client;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<EmailsDbContext>();

builder.Services.AddOpenApi();

// Email never publishes (at least not yet), only consumes - so, unlike
// Payment, this wiring stays in Program.cs rather than moving into
// Infrastructure. If Email ever needs to publish its own event, move it,
// same as Payment's did.
var rabbitMq = builder.Configuration.GetSection("RabbitMq");
builder.Services.AddSingleton(new ConnectionFactory
{
    HostName = rabbitMq["HostName"] ?? throw new InvalidOperationException("RabbitMq:HostName is not configured."),
    Port = int.Parse(rabbitMq["Port"] ?? "5672"),
    UserName = rabbitMq["UserName"] ?? throw new InvalidOperationException("RabbitMq:UserName is not configured."),
    Password = rabbitMq["Password"] ?? throw new InvalidOperationException("RabbitMq:Password is not configured.")
});
builder.Services.AddHostedService<PaymentOutcomeConsumer>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapHealthChecks("/health");
app.MapEmailEndpoints();

app.Run();
