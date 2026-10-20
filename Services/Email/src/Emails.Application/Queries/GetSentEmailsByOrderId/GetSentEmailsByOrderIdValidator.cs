using FluentValidation;

namespace Emails.Application.Queries.GetSentEmailsByOrderId;

public sealed class GetSentEmailsByOrderIdValidator : AbstractValidator<GetSentEmailsByOrderIdQuery>
{
    public GetSentEmailsByOrderIdValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
