using MediatR;
using Payments.Api.Contracts;
using Payments.Application.Commands.FailPayment;
using Payments.Application.Commands.CreatePayment;
using Payments.Application.Queries.GetPaymentById;
using Payments.Application.Commands.CompletePayment;
using Payments.Application.Queries.GetPaymentByOrderId;

namespace Payments.Api.Endpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/payments").WithTags("Payments");

        group.MapPost("/", async (CreatePaymentRequest request, ISender sender, CancellationToken ct) =>
        {
            var command = new CreatePaymentCommand(request.OrderId, request.Amount, request.Currency);
            var paymentId = await sender.Send(command, ct);
            return Results.Created($"/payments/{paymentId}", new { id = paymentId });
        });

        group.MapGet("/{paymentId:guid}", async (Guid paymentId, ISender sender, CancellationToken ct) =>
        {
            var payment = await sender.Send(new GetPaymentByIdQuery(paymentId), ct);
            return payment is null ? Results.NotFound() : Results.Ok(payment);
        });

        // Same resource as above, looked up by the order it belongs to instead
        // of its own id - the lookup an Order service call would actually use.
        group.MapGet("/", async (Guid? orderId, ISender sender, CancellationToken ct) =>
        {
            var payment = await sender.Send(new GetPaymentByOrderIdQuery(orderId ?? Guid.Empty), ct);
            return payment is null ? Results.NotFound() : Results.Ok(payment);
        });

        group.MapPost("/{paymentId:guid}/complete", async (Guid paymentId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new CompletePaymentCommand(paymentId), ct);
            return Results.NoContent();
        });

        group.MapPost("/{paymentId:guid}/fail", async (Guid paymentId, FailPaymentRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new FailPaymentCommand(paymentId, request.Reason), ct);
            return Results.NoContent();
        });
    }
}
