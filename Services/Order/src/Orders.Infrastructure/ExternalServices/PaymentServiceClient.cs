// SUPERSEDED (Phase 2): replaced by RabbitMqEventPublisher
// (Orders.Infrastructure/Messaging/RabbitMqEventPublisher.cs). Kept here,
// commented out, as a record of the Phase 1 synchronous-HTTP design this
// replaced. Safe to delete this whole file (and the ExternalServices folder,
// if nothing else lands in it) whenever you like.
//
// using System.Net;
// using System.Net.Http.Json;
// using Orders.Application.Abstractions;
// using Orders.Application.Common.Exceptions;
//
// namespace Orders.Infrastructure.ExternalServices;
//
// /// <summary>
// /// Talks to the Payment service over plain HTTP. No retries, no circuit
// /// breaker - Phase 3 (Polly) adds resilience. For now, a failure is just
// /// a failure, surfaced as PaymentServiceException -> 502.
// /// </summary>
// public sealed class PaymentServiceClient(HttpClient httpClient) : IPaymentServiceClient
// {
//     public async Task CreatePaymentAsync(Guid orderId, decimal amount, string currency, CancellationToken cancellationToken = default)
//     {
//         HttpResponseMessage response;
//
//         try
//         {
//             response = await httpClient.PostAsJsonAsync(
//                 "/payments",
//                 new CreatePaymentRequest(orderId, amount, currency),
//                 cancellationToken);
//         }
//         catch (HttpRequestException ex)
//         {
//             throw new PaymentServiceException("Could not reach the Payment service.", ex);
//         }
//         catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
//         {
//             // Caller didn't cancel - this is our own request timing out.
//             throw new PaymentServiceException("The Payment service did not respond in time.", ex);
//         }
//
//         // A payment already exists for this order - treat as already done, not an
//         // error, so retrying ConfirmOrder (e.g. after a dropped response) is safe.
//         if (response.StatusCode == HttpStatusCode.Conflict)
//             return;
//
//         if (!response.IsSuccessStatusCode)
//             throw new PaymentServiceException($"Payment service returned {(int)response.StatusCode} {response.StatusCode}.");
//     }
//
//     private sealed record CreatePaymentRequest(Guid OrderId, decimal Amount, string Currency);
// }
