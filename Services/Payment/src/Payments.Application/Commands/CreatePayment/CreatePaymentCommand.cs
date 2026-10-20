using MediatR;

namespace Payments.Application.Commands.CreatePayment;

public sealed record CreatePaymentCommand(Guid OrderId, decimal Amount, string Currency) : IRequest<Guid>;
