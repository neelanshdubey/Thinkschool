using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace QuotesApi.Tests;

// Unlike QuotesApiFactory, this factory keeps the real InternalJwt/EntraJwt
// authentication pipeline from Program.cs intact (no TestAuthHandler swap),
// so tests using it exercise genuine JWT issuance/validation end to end.
public class RealAuthQuotesApiFactory : WebApplicationFactory<Program>
{
    public string DbPath { get; } =
        Path.Combine(Path.GetTempPath(), $"quotesapi_realauth_tests_{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={DbPath}"
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        SqliteConnection.ClearAllPools();
        DeleteDbFiles();
    }

    private void DeleteDbFiles()
    {
        foreach (var suffix in new[] { "", "-shm", "-wal" })
        {
            var path = DbPath + suffix;

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
