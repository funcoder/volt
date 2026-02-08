using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Volt.Core;
using Volt.Core.Callbacks;
using Xunit;

namespace Volt.Data.Tests.Callbacks;

public class CallbackRunnerTests
{
    private class PlainModel : Model<PlainModel>
    {
        public string Name { get; init; } = "";
    }

    private class BeforeSaveModel : Model<BeforeSaveModel>, IBeforeSave
    {
        public string Name { get; init; } = "";
        public bool BeforeSaveCalled { get; private set; }

        public Task BeforeSaveAsync(CancellationToken cancellationToken = default)
        {
            BeforeSaveCalled = true;
            return Task.CompletedTask;
        }
    }

    private class TestDbContext : VoltDbContext
    {
        public DbSet<PlainModel> Plains => Set<PlainModel>();
        public DbSet<BeforeSaveModel> BeforeSaves => Set<BeforeSaveModel>();

        public TestDbContext(DbContextOptions options) : base(options) { }
    }

    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task PlainModel_NoCallbacksFired()
    {
        using var context = CreateContext();
        context.Plains.Add(new PlainModel { Name = "test" });

        var result = await context.SaveChangesAsync();

        result.Should().Be(1);
    }

    [Fact]
    public async Task BeforeSaveModel_CallbackFired_OnCreate()
    {
        using var context = CreateContext();
        var model = new BeforeSaveModel { Name = "test" };
        context.BeforeSaves.Add(model);

        await context.SaveChangesAsync();

        model.BeforeSaveCalled.Should().BeTrue();
    }

    [Fact]
    public async Task BeforeSaveModel_CallbackFired_OnUpdate()
    {
        using var context = CreateContext();
        var model = new BeforeSaveModel { Name = "original" };
        context.BeforeSaves.Add(model);
        await context.SaveChangesAsync();

        model.BeforeSaveCalled.Should().BeTrue();
    }
}
