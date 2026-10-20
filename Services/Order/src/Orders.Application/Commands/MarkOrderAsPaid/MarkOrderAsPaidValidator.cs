using FluentValidation;

namespace Orders.Application.Commands.MarkOrderAsPaid;

public sealed class MarkOrderAsPaidValidator : AbstractValidator<MarkOrderAsPaidCommand>
{
    public MarkOrderAsPaidValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
