using CollectionsApi.Models;
using CollectionsApi.Repositories;

namespace CollectionsApi.Services;

public class CollectionService : ICollectionService
{
    private readonly ICollectionRepository _repository;

    public CollectionService(ICollectionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Collection>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync(
            cancellationToken);
    }
}