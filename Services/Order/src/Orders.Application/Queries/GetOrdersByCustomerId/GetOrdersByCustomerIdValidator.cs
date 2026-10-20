using FluentValidation;

namespace Orders.Application.Queries.GetOrdersByCustomerId;

public sealed class GetOrdersByCustomerIdValidator : AbstractValidator<GetOrdersByCustomerIdQuery>
{
    public GetOrdersByCustomerIdValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}
