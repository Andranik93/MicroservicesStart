using FluentValidation;

namespace Payments.Application.Commands.FailPayment;

public sealed class FailPaymentValidator : AbstractValidator<FailPaymentCommand>
{
    public FailPaymentValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty();
    }
}
