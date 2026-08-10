using OrderRefactor.Data;
using OrderRefactor.Models;
using Microsoft.EntityFrameworkCore;

namespace OrderRefactor.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public OrderRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Order?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task AddAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        await _db.Orders.AddAsync(order, cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        await _db.SaveChangesAsync(cancellationToken);
    }
}

