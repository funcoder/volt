using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Volt.Data;
using Volt.Web.Middleware;
using Xunit;

namespace Volt.Web.Tests.Middleware;

public class PendingMigrationsMiddlewareTests
{
    [Fact]
    public async Task PassesThrough_WhenNoException()
    {
        var middleware = CreateMiddleware(_ => Task.CompletedTask);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task RethrowsException_WhenNonDatabaseException()
    {
        var middleware = CreateMiddleware(_ =>
            throw new InvalidOperationException("not a db error"));
        var context = CreateHttpContext();

        var act = () => middleware.InvokeAsync(context);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("not a db error");
    }

    [Fact]
    public async Task RethrowsSqliteException_WhenNoVoltDbContextRegistered()
    {
        var sqliteEx = CreateSqliteException();
        var middleware = CreateMiddleware(_ => throw sqliteEx);
        var context = CreateHttpContext();

        var act = () => middleware.InvokeAsync(context);

        await act.Should().ThrowAsync<SqliteException>();
    }

    [Fact]
    public async Task Returns503_WhenSqliteExceptionAndPendingMigrations()
    {
        var sqliteEx = CreateSqliteException();
        var middleware = CreateMiddleware(_ => throw sqliteEx);
        var context = CreateHttpContextWithPendingMigrations();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(503);
        context.Response.ContentType.Should().Contain("text/html");
    }

    [Fact]
    public async Task RendersPageWithMigrationId_WhenPendingMigrationsExist()
    {
        var sqliteEx = CreateSqliteException();
        var middleware = CreateMiddleware(_ => throw sqliteEx);
        var context = CreateHttpContextWithPendingMigrations();

        await middleware.InvokeAsync(context);

        var body = await ReadResponseBody(context);
        body.Should().Contain("20250101000000_CreateTestItems");
        body.Should().Contain("volt db migrate");
    }

    [Fact]
    public async Task RendersPageWithPendingCount()
    {
        var sqliteEx = CreateSqliteException();
        var middleware = CreateMiddleware(_ => throw sqliteEx);
        var context = CreateHttpContextWithPendingMigrations();

        await middleware.InvokeAsync(context);

        var body = await ReadResponseBody(context);
        body.Should().Contain("Pending Migrations");
    }

    [Fact]
    public async Task CatchesException_WhenInnerExceptionIsSqliteError()
    {
        var inner = CreateSqliteException();
        var wrapper = new Exception("outer", inner);
        var middleware = CreateMiddleware(_ => throw wrapper);
        var context = CreateHttpContextWithPendingMigrations();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(503);
    }

    [Fact]
    public async Task RethrowsWrappedException_WhenInnerIsSqliteButNoPendingMigrations()
    {
        var inner = CreateSqliteException();
        var wrapper = new Exception("outer", inner);
        var middleware = CreateMiddleware(_ => throw wrapper);
        var context = CreateHttpContext();

        var act = () => middleware.InvokeAsync(context);

        await act.Should().ThrowAsync<Exception>().WithMessage("outer");
    }

    private static PendingMigrationsMiddleware CreateMiddleware(RequestDelegate next) =>
        new(next);

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.RequestServices = new ServiceCollection().BuildServiceProvider();
        return context;
    }

    private static DefaultHttpContext CreateHttpContextWithPendingMigrations()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddDbContext<TestMigrationDbContext>(opts =>
            opts.UseSqlite(connection));
        services.AddScoped<VoltDbContext>(sp =>
            sp.GetRequiredService<TestMigrationDbContext>());

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.RequestServices = services.BuildServiceProvider();
        return context;
    }

    private static async Task<string> ReadResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }

    private static SqliteException CreateSqliteException()
    {
        try
        {
            using var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM non_existent_table";
            cmd.ExecuteReader();
        }
        catch (SqliteException ex)
        {
            return ex;
        }

        throw new InvalidOperationException("Expected SqliteException was not thrown");
    }
}

/// <summary>
/// Concrete DbContext used by tests to simulate pending migrations.
/// </summary>
public class TestMigrationDbContext : VoltDbContext
{
    public TestMigrationDbContext(DbContextOptions<TestMigrationDbContext> options)
        : base(options) { }
}

/// <summary>
/// A migration associated with TestMigrationDbContext that will appear as "pending"
/// when the database has not been migrated.
/// </summary>
[DbContext(typeof(TestMigrationDbContext))]
[Migration("20250101000000_CreateTestItems")]
public class CreateTestItems : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "test_items",
            columns: table => new
            {
                id = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_test_items", x => x.id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "test_items");
    }
}
