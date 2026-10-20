using FluentValidation;

namespace Orders.Application.Commands.MarkOrderPaymentFailed;

public sealed class MarkOrderPaymentFailedValidator : AbstractValidator<MarkOrderPaymentFailedCommand>
{
    public MarkOrderPaymentFailedValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty();
    }
}
