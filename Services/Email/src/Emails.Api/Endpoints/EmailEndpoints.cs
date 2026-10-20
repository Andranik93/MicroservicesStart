using Emails.Application.Queries.GetSentEmailsByOrderId;
using MediatR;

namespace Emails.Api.Endpoints;

public static class EmailEndpoints
{
    public static void MapEmailEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/emails").WithTags("Emails");

        // The only endpoint this service needs: a way to verify what was
        // sent for an order, without having to open Mailpit. Nothing else
        // is exposed over HTTP - sending only ever happens by reacting to
        // a real payment outcome event.
        group.MapGet("/", async (Guid? orderId, ISender sender, CancellationToken ct) =>
        {
            var sentEmails = await sender.Send(new GetSentEmailsByOrderIdQuery(orderId ?? Guid.Empty), ct);
            return Results.Ok(sentEmails);
        });
    }
}
