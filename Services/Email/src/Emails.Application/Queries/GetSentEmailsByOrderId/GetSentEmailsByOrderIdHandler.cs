using Emails.Application.Abstractions;
using MediatR;

namespace Emails.Application.Queries.GetSentEmailsByOrderId;

public sealed class GetSentEmailsByOrderIdHandler(IEmailRepository emails)
    : IRequestHandler<GetSentEmailsByOrderIdQuery, IReadOnlyCollection<SentEmailResponse>>
{
    public async Task<IReadOnlyCollection<SentEmailResponse>> Handle(
        GetSentEmailsByOrderIdQuery request,
        CancellationToken cancellationToken)
    {
        var sentEmails = await emails.GetByOrderIdAsync(request.OrderId, cancellationToken);

        return sentEmails
            .Select(e => new SentEmailResponse(e.Id, e.OrderId, e.Kind.ToString(), e.ToAddress, e.Subject, e.SentAtUtc))
            .ToList();
    }
}
