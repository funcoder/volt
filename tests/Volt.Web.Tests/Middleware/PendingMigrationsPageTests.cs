using FluentAssertions;
using Volt.Web.Middleware;
using Xunit;

namespace Volt.Web.Tests.Middleware;

public class PendingMigrationsPageTests
{
    [Fact]
    public void Render_IncludesMigrationIds()
    {
        var migrations = new List<string>
        {
            "20250101000000_CreateUsers",
            "20250102000000_CreatePosts"
        };

        var html = PendingMigrationsPage.Render(migrations);

        html.Should().Contain("20250101000000_CreateUsers");
        html.Should().Contain("20250102000000_CreatePosts");
    }

    [Fact]
    public void Render_IncludesFormattedNames()
    {
        var migrations = new List<string> { "20250101000000_CreateBlogPosts" };

        var html = PendingMigrationsPage.Render(migrations);

        html.Should().Contain("Create Blog Posts");
    }

    [Fact]
    public void Render_IncludesMigrationCount()
    {
        var migrations = new List<string>
        {
            "20250101_A",
            "20250102_B",
            "20250103_C"
        };

        var html = PendingMigrationsPage.Render(migrations);

        html.Should().Contain("3");
    }

    [Fact]
    public void Render_IncludesVoltDbMigrateCommand()
    {
        var migrations = new List<string> { "20250101000000_Init" };

        var html = PendingMigrationsPage.Render(migrations);

        html.Should().Contain("volt db migrate");
    }

    [Fact]
    public void Render_ReturnsValidHtml()
    {
        var migrations = new List<string> { "20250101000000_Test" };

        var html = PendingMigrationsPage.Render(migrations);

        html.Should().Contain("<!DOCTYPE html>");
        html.Should().Contain("</html>");
        html.Should().Contain("Pending Migrations");
    }
}
