using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceTiming.Api.Data;

namespace RaceTiming.Tests;

/// <summary>
/// Boots the real app (real middleware, real controllers, real migrations) against a
/// private in-memory SQLite connection per test class instance, instead of the file-based
/// one from appsettings — so tests are isolated and leave nothing on disk.
/// </summary>
public class RaceTimingApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public RaceTimingApiFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<RaceTimingDbContext>>();
            services.AddDbContext<RaceTimingDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}
