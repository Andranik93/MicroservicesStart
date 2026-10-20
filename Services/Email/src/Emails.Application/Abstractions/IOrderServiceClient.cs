namespace Emails.Application.Abstractions;

/// <summary>
/// Read-only call to the Order service to enrich a notification - neither
/// OrderConfirmedEvent nor PaymentCompletedEvent/PaymentFailedEvent carry a
/// CustomerId or email address, and there's no User/Auth service yet to get
/// a real address from either.
///
/// This is a synchronous cross-service call, like the one Order used to make
/// to Payment - but that one was a WRITE on the critical path: its failure
/// blocked confirming an order. This is a READ, used only to format a
/// notification: if it fails, the email is skipped and logged, nothing else
/// breaks. That's why a synchronous call is an acceptable choice here even
/// though messaging replaced the other one.
/// </summary>
public interface IOrderServiceClient
{
    Task<OrderDetails?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}

public sealed record OrderDetails(Guid Id, Guid CustomerId, decimal Total, string Currency);
