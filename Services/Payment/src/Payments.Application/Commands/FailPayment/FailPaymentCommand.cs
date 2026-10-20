using MediatR;

namespace Payments.Application.Commands.FailPayment;

public sealed record FailPaymentCommand(Guid PaymentId, string Reason) : IRequest;
