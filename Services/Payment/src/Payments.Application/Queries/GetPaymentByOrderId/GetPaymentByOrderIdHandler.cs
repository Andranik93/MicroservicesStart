using MediatR;
using Payments.Application.Abstractions;
using Payments.Application.Queries.GetPaymentById;
using Payments.Application.Queries.Mapping;

namespace Payments.Application.Queries.GetPaymentByOrderId;

public sealed class GetPaymentByOrderIdHandler(IPaymentRepository payments)
    : IRequestHandler<GetPaymentByOrderIdQuery, PaymentResponse?>
{
    public async Task<PaymentResponse?> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await payments.GetByOrderIdAsync(request.OrderId, cancellationToken);

        return payment is null ? null : PaymentMapper.ToResponse(payment);
    }
}
