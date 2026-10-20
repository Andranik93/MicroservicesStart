// SUPERSEDED (Phase 2): nothing throws this anymore, since Order no longer
// calls Payment synchronously over HTTP (see IPaymentServiceClient.cs in this
// same state). Kept here, commented out, as a record of the Phase 1 design
// this replaced. Safe to delete entirely whenever you like.
//
// namespace Orders.Application.Common.Exceptions;
//
// /// <summary>
// /// Thrown when the Payment service can't be reached or returns an
// /// unexpected response. Lets the API return 502 Bad Gateway - this request
// /// was fine, but a service it depends on was not.
// /// </summary>
// public sealed class PaymentServiceException(string message, Exception? innerException = null)
//     : Exception(message, innerException);
