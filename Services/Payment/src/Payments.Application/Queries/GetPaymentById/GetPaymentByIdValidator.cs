using FluentValidation;

namespace Payments.Application.Queries.GetPaymentById;

public sealed class GetPaymentByIdValidator : AbstractValidator<GetPaymentByIdQuery>
{
    public GetPaymentByIdValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
    }
}
