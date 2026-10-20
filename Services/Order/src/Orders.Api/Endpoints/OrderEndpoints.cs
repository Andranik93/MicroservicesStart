using MediatR;
using Orders.Api.Contracts;
using Orders.Application.Queries.GetOrderById;
using Orders.Application.Commands.CreateOrder;
using Orders.Application.Commands.ConfirmOrder;
using Orders.Application.Commands.AddOrderItem;
using Orders.Application.Queries.GetOrdersByCustomerId;

namespace Orders.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orders").WithTags("Orders");

        group.MapPost("/", async (CreateOrderRequest request, ISender sender, CancellationToken ct) =>
        {
            var orderId = await sender.Send(new CreateOrderCommand(request.CustomerId, request.Currency), ct);
            return Results.Created($"/orders/{orderId}", new { id = orderId });
        });

        group.MapPost("/{orderId:guid}/items", async (Guid orderId, AddOrderItemRequest request, ISender sender, CancellationToken ct) =>
        {
            var command = new AddOrderItemCommand(orderId, request.ProductId, request.UnitPrice, request.Quantity);
            await sender.Send(command, ct);
            return Results.NoContent();
        });

        group.MapPost("/{orderId:guid}/confirm", async (Guid orderId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new ConfirmOrderCommand(orderId), ct);
            return Results.NoContent();
        });

        group.MapGet("/{orderId:guid}", async (Guid orderId, ISender sender, CancellationToken ct) =>
        {
            var order = await sender.Send(new GetOrderByIdQuery(orderId), ct);
            return order is null ? Results.NotFound() : Results.Ok(order);
        });

        // Same resource as above, filtered by customer instead of looked up by its own id.
        // customerId is nullable here on purpose: a missing query parameter should fail
        // validation like any other bad input (consistent 400 shape), not throw a raw
        // framework exception for a *required* typed parameter.
        group.MapGet("/", async (Guid? customerId, ISender sender, CancellationToken ct) =>
        {
            var orders = await sender.Send(new GetOrdersByCustomerIdQuery(customerId ?? Guid.Empty), ct);
            return Results.Ok(orders);
        });


    }
}
