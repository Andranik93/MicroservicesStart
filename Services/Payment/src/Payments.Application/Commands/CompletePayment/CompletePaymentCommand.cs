using MediatR;

namespace Payments.Application.Commands.CompletePayment;

public sealed record CompletePaymentCommand(Guid PaymentId) : IRequest;
