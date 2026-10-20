namespace Payments.Application.Queries.GetPaymentById;

public sealed record PaymentResponse(
    Guid Id,
    Guid OrderId,
    decimal Amount,
    string Currency,
    string Status,
    string? FailureReason);
