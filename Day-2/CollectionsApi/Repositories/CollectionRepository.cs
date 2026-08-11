using CollectionsApi.Data;
using CollectionsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CollectionsApi.Repositories;

public class CollectionRepository : ICollectionRepository
{
    private readonly AppDbContext _db;

    public CollectionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Collection>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _db.Collections
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);
    }
}