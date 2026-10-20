using MediatR;
using Payments.Application.Queries.GetPaymentById;

namespace Payments.Application.Queries.GetPaymentByOrderId;

public sealed record GetPaymentByOrderIdQuery(Guid OrderId) : IRequest<PaymentResponse?>;
