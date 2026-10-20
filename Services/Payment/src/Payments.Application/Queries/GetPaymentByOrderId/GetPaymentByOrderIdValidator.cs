using FluentValidation;

namespace Payments.Application.Queries.GetPaymentByOrderId;

public sealed class GetPaymentByOrderIdValidator : AbstractValidator<GetPaymentByOrderIdQuery>
{
    public GetPaymentByOrderIdValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
