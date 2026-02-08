using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Volt.Web.Middleware;
using Xunit;

namespace Volt.Web.Tests.Middleware;

public class VoltMiddlewareExtensionsTests
{
    [Fact]
    public void UseVolt_ConfiguresPipeline_WithoutError()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddControllersWithViews();
        var app = builder.Build();

        var act = () => app.UseVolt();

        act.Should().NotThrow();
    }

    [Fact]
    public void UseVolt_ReturnsWebApplication_ForChaining()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddControllersWithViews();
        var app = builder.Build();

        var result = app.UseVolt();

        result.Should().BeSameAs(app);
    }
}
