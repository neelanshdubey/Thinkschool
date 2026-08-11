using CollectionsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CollectionsApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Collection> Collections => Set<Collection>();
}