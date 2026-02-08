using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using FluentAssertions;
using Volt.Storage;
using Xunit;

namespace Volt.Data.Tests;

public class VoltAttachmentColumnMappingTests
{
    [Fact]
    public void HasTableAttribute_WithSnakeCaseName()
    {
        var attr = typeof(VoltAttachment).GetCustomAttribute<TableAttribute>();

        attr.Should().NotBeNull();
        attr!.Name.Should().Be("volt_attachments");
    }

    [Theory]
    [InlineData(nameof(VoltAttachment.Id), "id")]
    [InlineData(nameof(VoltAttachment.Filename), "filename")]
    [InlineData(nameof(VoltAttachment.ContentType), "content_type")]
    [InlineData(nameof(VoltAttachment.ByteSize), "byte_size")]
    [InlineData(nameof(VoltAttachment.Key), "key")]
    [InlineData(nameof(VoltAttachment.ServiceName), "service_name")]
    [InlineData(nameof(VoltAttachment.Checksum), "checksum")]
    [InlineData(nameof(VoltAttachment.CreatedAt), "created_at")]
    public void Property_HasCorrectColumnAttribute(string propertyName, string expectedColumnName)
    {
        var property = typeof(VoltAttachment).GetProperty(propertyName);
        property.Should().NotBeNull($"property {propertyName} should exist");

        var attr = property!.GetCustomAttribute<ColumnAttribute>();
        attr.Should().NotBeNull($"property {propertyName} should have [Column] attribute");
        attr!.Name.Should().Be(expectedColumnName);
    }

    [Fact]
    public void AllProperties_HaveColumnAttributes()
    {
        var properties = typeof(VoltAttachment)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            var attr = prop.GetCustomAttribute<ColumnAttribute>();
            attr.Should().NotBeNull($"property {prop.Name} should have [Column] attribute");
        }
    }

    [Fact]
    public void AllColumnNames_AreSnakeCase()
    {
        var properties = typeof(VoltAttachment)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            var attr = prop.GetCustomAttribute<ColumnAttribute>();
            attr.Should().NotBeNull();

            var name = attr!.Name!;
            name.Should().MatchRegex("^[a-z][a-z0-9_]*$",
                $"column name for {prop.Name} should be snake_case");
        }
    }

    [Fact]
    public void AllProperties_UseInitSetters()
    {
        var properties = typeof(VoltAttachment)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            var setter = prop.SetMethod;
            setter.Should().NotBeNull($"property {prop.Name} should have a setter");

            // Init-only setters have IsExternalInit in their return parameter
            var isInitOnly = setter!.ReturnParameter
                .GetRequiredCustomModifiers()
                .Any(t => t.Name == "IsExternalInit");

            isInitOnly.Should().BeTrue($"property {prop.Name} should use init setter");
        }
    }
}
