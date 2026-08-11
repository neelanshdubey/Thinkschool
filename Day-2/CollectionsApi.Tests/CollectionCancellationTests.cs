using CollectionsApi.Models;
using CollectionsApi.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CollectionsApi.Tests;

public class CollectionCancellationTests
{
    [Fact]
    public async Task GetCollections_WhenRequestIsCancelled_DoesNotComplete()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<ICollectionRepository>();

                    services.AddScoped<
                        ICollectionRepository,
                        BlockingCollectionRepository>();
                });
            });

        using var client = factory.CreateClient();

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var requestTask = client.GetAsync(
            "/api/collections",
            cancellationTokenSource.Token);

        await Task.Delay(100);

        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await requestTask);
    }

    private sealed class BlockingCollectionRepository
        : ICollectionRepository
    {
        public async Task<List<Collection>> GetAllAsync(
            CancellationToken cancellationToken)
        {
            await Task.Delay(
                Timeout.InfiniteTimeSpan,
                cancellationToken);

            return [];
        }
    }
}