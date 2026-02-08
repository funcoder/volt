using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Volt.Core;
using Volt.Core.Callbacks;
using Xunit;

namespace Volt.Data.Tests.Callbacks;

public class CallbackIntegrationTests
{
    private readonly List<string> _callLog = [];

    private class Article : Model<Article>, IBeforeSave, IAfterSave, IBeforeCreate, IAfterCreate,
        IBeforeUpdate, IAfterUpdate, IBeforeDestroy, IAfterDestroy
    {
        public string Title { get; init; } = "";
        public string Slug { get; init; } = "";

        // Shared log for assertion — set by the test fixture before each save
        internal List<string>? CallLog { get; set; }

        public Task BeforeSaveAsync(CancellationToken cancellationToken = default)
        {
            CallLog?.Add("BeforeSave");
            return Task.CompletedTask;
        }

        public Task AfterSaveAsync(CancellationToken cancellationToken = default)
        {
            CallLog?.Add("AfterSave");
            return Task.CompletedTask;
        }

        public Task BeforeCreateAsync(CancellationToken cancellationToken = default)
        {
            CallLog?.Add("BeforeCreate");
            return Task.CompletedTask;
        }

        public Task AfterCreateAsync(CancellationToken cancellationToken = default)
        {
            CallLog?.Add("AfterCreate");
            return Task.CompletedTask;
        }

        public Task BeforeUpdateAsync(CancellationToken cancellationToken = default)
        {
            CallLog?.Add("BeforeUpdate");
            return Task.CompletedTask;
        }

        public Task AfterUpdateAsync(CancellationToken cancellationToken = default)
        {
            CallLog?.Add("AfterUpdate");
            return Task.CompletedTask;
        }

        public Task BeforeDestroyAsync(CancellationToken cancellationToken = default)
        {
            CallLog?.Add("BeforeDestroy");
            return Task.CompletedTask;
        }

        public Task AfterDestroyAsync(CancellationToken cancellationToken = default)
        {
            CallLog?.Add("AfterDestroy");
            return Task.CompletedTask;
        }
    }

    private class ThrowingModel : Model<ThrowingModel>, IBeforeSave
    {
        public string Name { get; init; } = "";

        public Task BeforeSaveAsync(CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Callback failed");
        }
    }

    private class TestDbContext : VoltDbContext
    {
        public DbSet<Article> Articles => Set<Article>();
        public DbSet<ThrowingModel> ThrowingModels => Set<ThrowingModel>();

        public TestDbContext(DbContextOptions options, IOptions<VoltDbOptions> voltOptions)
            : base(options, voltOptions) { }
    }

    private static TestDbContext CreateContext(Action<VoltDbOptions>? configure = null)
    {
        var voltOptions = new VoltDbOptions();
        // Disable timestamps to avoid CURRENT_TIMESTAMP default which fails with InMemory
        voltOptions.Timestamps(false);
        configure?.Invoke(voltOptions);

        var dbOptions = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new TestDbContext(dbOptions, Options.Create(voltOptions));
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task Create_FiresCallbacksInCorrectOrder()
    {
        using var context = CreateContext();
        var article = new Article { Title = "Hello", CallLog = _callLog };
        context.Articles.Add(article);

        await context.SaveChangesAsync();

        _callLog.Should().Equal("BeforeSave", "BeforeCreate", "AfterCreate", "AfterSave");
    }

    [Fact]
    public async Task Update_FiresCallbacksInCorrectOrder()
    {
        using var context = CreateContext();
        var article = new Article { Title = "Hello" };
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        article.CallLog = _callLog;
        context.Entry(article).Property(a => a.Title).CurrentValue = "Updated";
        context.Entry(article).State = EntityState.Modified;

        await context.SaveChangesAsync();

        _callLog.Should().Equal("BeforeSave", "BeforeUpdate", "AfterUpdate", "AfterSave");
    }

    [Fact]
    public async Task Delete_FiresDestroyCallbacks()
    {
        using var context = CreateContext();
        var article = new Article { Title = "Hello" };
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        article.CallLog = _callLog;
        context.Articles.Remove(article);

        await context.SaveChangesAsync();

        _callLog.Should().Equal("BeforeDestroy", "AfterDestroy");
    }

    [Fact]
    public async Task CallbacksDisabled_NoCallbacksFired()
    {
        using var context = CreateContext(opts => opts.Callbacks(false));
        var article = new Article { Title = "Hello", CallLog = _callLog };
        context.Articles.Add(article);

        await context.SaveChangesAsync();

        _callLog.Should().BeEmpty();
    }

    [Fact]
    public async Task CallbackException_AbortsSave()
    {
        using var context = CreateContext();
        context.ThrowingModels.Add(new ThrowingModel { Name = "boom" });

        var act = () => context.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Callback failed");

        context.ThrowingModels.ToList().Should().BeEmpty();
    }

    [Fact]
    public void SyncSaveChanges_FiresCallbacks()
    {
        using var context = CreateContext();
        var article = new Article { Title = "Hello", CallLog = _callLog };
        context.Articles.Add(article);

        context.SaveChanges();

        _callLog.Should().Equal("BeforeSave", "BeforeCreate", "AfterCreate", "AfterSave");
    }

    [Fact]
    public async Task MultipleEntities_AllCallbacksFire()
    {
        using var context = CreateContext();
        var article1 = new Article { Title = "First", CallLog = _callLog };
        var article2 = new Article { Title = "Second", CallLog = _callLog };
        context.Articles.Add(article1);
        context.Articles.Add(article2);

        await context.SaveChangesAsync();

        _callLog.Where(c => c == "BeforeSave").Should().HaveCount(2);
        _callLog.Where(c => c == "BeforeCreate").Should().HaveCount(2);
        _callLog.Where(c => c == "AfterCreate").Should().HaveCount(2);
        _callLog.Where(c => c == "AfterSave").Should().HaveCount(2);
    }
}
