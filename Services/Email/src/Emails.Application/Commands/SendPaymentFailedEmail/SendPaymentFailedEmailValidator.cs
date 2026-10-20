using FluentValidation;

namespace Emails.Application.Commands.SendPaymentFailedEmail;

public sealed class SendPaymentFailedEmailValidator : AbstractValidator<SendPaymentFailedEmailCommand>
{
    public SendPaymentFailedEmailValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty();
    }
}
