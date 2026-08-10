using OrderRefactor.Models;

namespace OrderRefactor.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task AddAsync(
        Order order,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}

