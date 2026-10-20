using MediatR;

namespace Emails.Application.Commands.SendPaymentFailedEmail;

public sealed record SendPaymentFailedEmailCommand(Guid OrderId, string Reason) : IRequest;
