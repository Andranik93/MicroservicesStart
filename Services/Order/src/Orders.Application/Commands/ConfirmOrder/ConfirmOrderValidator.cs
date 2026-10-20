using FluentValidation;

namespace Orders.Application.Commands.ConfirmOrder;

public sealed class ConfirmOrderValidator : AbstractValidator<ConfirmOrderCommand>
{
    public ConfirmOrderValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
