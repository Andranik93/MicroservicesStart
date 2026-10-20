// SUPERSEDED (Phase 2): Order no longer calls Payment synchronously over HTTP.
// ConfirmOrderHandler now publishes OrderConfirmedEvent via IEventPublisher
// instead. Kept here, commented out, as a record of the Phase 1 design this
// replaced - see RabbitMqEventPublisher and ConfirmOrderHandler for the
// current approach. Safe to delete entirely whenever you like.
//
// namespace Orders.Application.Abstractions;
//
// /// <summary>
// /// Outbound call to the Payment service. Defined here (dependency inversion),
// /// implemented in Infrastructure with HttpClient - the Application layer
// /// knows nothing about HTTP.
// /// </summary>
// public interface IPaymentServiceClient
// {
//     Task CreatePaymentAsync(Guid orderId, decimal amount, string currency, CancellationToken cancellationToken = default);
// }
