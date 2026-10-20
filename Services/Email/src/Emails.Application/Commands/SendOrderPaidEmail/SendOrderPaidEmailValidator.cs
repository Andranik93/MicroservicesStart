using FluentValidation;

namespace Emails.Application.Commands.SendOrderPaidEmail;

public sealed class SendOrderPaidEmailValidator : AbstractValidator<SendOrderPaidEmailCommand>
{
    public SendOrderPaidEmailValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
