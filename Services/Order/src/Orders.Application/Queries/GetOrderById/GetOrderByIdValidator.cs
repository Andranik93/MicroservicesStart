using FluentValidation;

namespace Orders.Application.Queries.GetOrderById;

public sealed class GetOrderByIdValidator : AbstractValidator<GetOrderByIdQuery>
{
    public GetOrderByIdValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
