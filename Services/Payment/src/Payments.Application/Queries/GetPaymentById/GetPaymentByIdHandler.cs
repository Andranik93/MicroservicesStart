using MediatR;
using Payments.Application.Abstractions;
using Payments.Application.Queries.Mapping;

namespace Payments.Application.Queries.GetPaymentById;

public sealed class GetPaymentByIdHandler(IPaymentRepository payments)
    : IRequestHandler<GetPaymentByIdQuery, PaymentResponse?>
{
    public async Task<PaymentResponse?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await payments.GetByIdAsync(request.PaymentId, cancellationToken);

        return payment is null ? null : PaymentMapper.ToResponse(payment);
    }
}
