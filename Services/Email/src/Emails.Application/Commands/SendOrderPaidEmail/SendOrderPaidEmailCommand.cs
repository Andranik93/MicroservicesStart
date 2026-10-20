using MediatR;

namespace Emails.Application.Commands.SendOrderPaidEmail;

// No HTTP endpoint maps to this on purpose - only ever reached by reacting
// to a real PaymentCompletedEvent, same rule as Order's MarkOrderAsPaidCommand.
public sealed record SendOrderPaidEmailCommand(Guid OrderId) : IRequest;
