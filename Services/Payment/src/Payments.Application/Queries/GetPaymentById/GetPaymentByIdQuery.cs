using MediatR;

namespace Payments.Application.Queries.GetPaymentById;

public sealed record GetPaymentByIdQuery(Guid PaymentId) : IRequest<PaymentResponse?>;
