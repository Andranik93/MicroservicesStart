using MediatR;

namespace Orders.Application.Commands.MarkOrderAsPaid;

// No HTTP endpoint maps to this on purpose - it is only ever reached by
// reacting to a real PaymentCompletedEvent, never called directly.
public sealed record MarkOrderAsPaidCommand(Guid OrderId) : IRequest;
