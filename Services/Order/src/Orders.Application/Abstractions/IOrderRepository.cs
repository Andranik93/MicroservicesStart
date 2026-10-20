using Orders.Domain.OrderAggregate;

namespace Orders.Application.Abstractions;

/// <summary>
/// Persistence contract defined by the Application layer. Infrastructure implements it,
/// so the use cases never depend on EF Core.
/// </summary>
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task AddAsync(Order order, CancellationToken cancellationToken = default);
}
