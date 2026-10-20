using MediatR;

namespace Emails.Application.Queries.GetSentEmailsByOrderId;

public sealed record GetSentEmailsByOrderIdQuery(Guid OrderId) : IRequest<IReadOnlyCollection<SentEmailResponse>>;
