using FluentValidation;

namespace Payments.Application.Commands.CompletePayment;

public sealed class CompletePaymentValidator : AbstractValidator<CompletePaymentCommand>
{
    public CompletePaymentValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
    }
}
