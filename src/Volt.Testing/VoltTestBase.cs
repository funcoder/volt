using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volt.Data;
using Volt.Testing.Factories;
using Xunit;

namespace Volt.Testing;

/// <summary>
/// Base class for Volt integration tests. Sets up a <see cref="WebApplicationFactory{TEntryPoint}"/>
/// with an in-memory database, provides <see cref="HttpClient"/> and <see cref="VoltDbContext"/> access,
/// and handles database cleanup between tests.
/// </summary>
/// <typeparam name="TProgram">The entry point class of the application under test (typically <c>Program</c>).</typeparam>
/// <typeparam name="TContext">The <see cref="VoltDbContext"/> subclass used by the application.</typeparam>
public abstract class VoltTestBase<TProgram, TContext> : IAsyncLifetime
    where TProgram : class
    where TContext : VoltDbContext
{
    private WebApplicationFactory<TProgram>? _factory;
    private IServiceScope? _scope;

    /// <summary>
    /// Gets the <see cref="HttpClient"/> configured for the test server.
    /// </summary>
    protected HttpClient Client { get; private set; } = null!;

    /// <summary>
    /// Gets the scoped <see cref="VoltDbContext"/> for direct database access in tests.
    /// </summary>
    protected TContext Db { get; private set; } = null!;

    /// <summary>
    /// Gets the scoped <see cref="IServiceProvider"/> for resolving services in tests.
    /// </summary>
    protected IServiceProvider Services => _scope!.ServiceProvider;

    /// <summary>
    /// Initializes the test server, in-memory database, and test client.
    /// Called automatically by xUnit before each test.
    /// </summary>
    public virtual Task InitializeAsync()
    {
        var databaseName = $"VoltTest_{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<TProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                ConfigureWebHost(builder);
                ReplaceDbContextWithInMemory(builder, databaseName);
            });

        Client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        Db = _scope.ServiceProvider.GetRequiredService<TContext>();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Cleans up the test server, database, and factory definitions.
    /// Called automatically by xUnit after each test.
    /// </summary>
    public virtual Task DisposeAsync()
    {
        Client?.Dispose();
        _scope?.Dispose();
        _factory?.Dispose();
        Factory.Reset();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Override to add custom service configuration for the test host.
    /// </summary>
    /// <param name="builder">The web host builder to configure.</param>
    protected virtual void ConfigureWebHost(IWebHostBuilder builder)
    {
    }

    private static void ReplaceDbContextWithInMemory(IWebHostBuilder builder, string databaseName)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<TContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
            });
        });
    }
}
