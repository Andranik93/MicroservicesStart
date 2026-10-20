namespace Payments.Api.Contracts;

public sealed record CreatePaymentRequest(Guid OrderId, decimal Amount, string Currency);
