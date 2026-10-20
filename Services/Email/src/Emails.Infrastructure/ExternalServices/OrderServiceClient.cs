using System.Net;
using System.Net.Http.Json;
using Emails.Application.Abstractions;

namespace Emails.Infrastructure.ExternalServices;

/// <summary>
/// Read-only GET /orders/{id} on Order.Api - see IOrderServiceClient for why
/// a synchronous call is an acceptable choice here.
/// </summary>
public sealed class OrderServiceClient(HttpClient httpClient) : IOrderServiceClient
{
    public async Task<OrderDetails?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;

        try
        {
            response = await httpClient.GetAsync($"/orders/{orderId}", cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null; // Order.Api unreachable - caller logs and skips the email.
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
            return null;

        var body = await response.Content.ReadFromJsonAsync<OrderApiResponse>(cancellationToken);
        return body is null ? null : new OrderDetails(body.Id, body.CustomerId, body.Total, body.Currency);
    }

    // Order.Api's OrderResponse has more fields (Status, Items, ...) - this
    // only reads the ones Email actually needs. Email's own copy of the
    // wire contract, same duplication rule as every integration event.
    private sealed record OrderApiResponse(Guid Id, Guid CustomerId, string Currency, decimal Total);
}
