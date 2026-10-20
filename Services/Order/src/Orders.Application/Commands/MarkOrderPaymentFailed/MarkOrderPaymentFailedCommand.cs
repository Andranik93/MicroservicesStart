using MediatR;

namespace Orders.Application.Commands.MarkOrderPaymentFailed;

// No HTTP endpoint maps to this on purpose - it is only ever reached by
// reacting to a real PaymentFailedEvent, never called directly.
public sealed record MarkOrderPaymentFailedCommand(Guid OrderId, string Reason) : IRequest;
