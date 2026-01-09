using DataverseDoc.Core.Models;
using DataverseDoc.Renderers;
using FluentAssertions;
using Xunit;

namespace DataverseDoc.Core.Tests.Renderers;

public class JsonRendererTests
{
    [Fact]
    public void Render_WithEnvironmentVariables_ProducesValidJson()
    {
        // Arrange
        var renderer = new JsonRenderer();
        var data = new List<EnvironmentVariable>
        {
            new("Test Variable", "test_var", "current", "default", "String")
        };
        using var writer = new StringWriter();

        // Act
        renderer.Render(data, writer);
        var result = writer.ToString();

        // Assert
        result.Should().Contain("\"displayName\"");
        result.Should().Contain("\"Test Variable\"");
        result.Should().Contain("\"schemaName\"");
        result.Should().Contain("\"test_var\"");
    }

    [Fact]
    public void Render_WithEmptyList_ProducesEmptyJsonArray()
    {
        // Arrange
        var renderer = new JsonRenderer();
        var data = new List<EnvironmentVariable>();
        using var writer = new StringWriter();

        // Act
        renderer.Render(data, writer);
        var result = writer.ToString().Trim();

        // Assert
        result.Should().Be("[]");
    }

    [Fact]
    public void Render_WithNullValues_ExcludesNullsFromJson()
    {
        // Arrange
        var renderer = new JsonRenderer();
        var data = new List<EnvironmentVariable>
        {
            new("Test Variable", "test_var", null, null, "String")
        };
        using var writer = new StringWriter();

        // Act
        renderer.Render(data, writer);
        var result = writer.ToString();

        // Assert
        result.Should().NotContain("\"currentValue\": null");
        result.Should().NotContain("\"defaultValue\": null");
    }

    [Fact]
    public void RenderSingle_WithEntityMetadata_ProducesValidJson()
    {
        // Arrange
        var renderer = new JsonRenderer();
        var data = new EntityMetadata(
            "account",
            "Account",
            new List<EntityRelationship>
            {
                new("account", "contact", "OneToMany", "account_contacts")
            });
        using var writer = new StringWriter();

        // Act
        renderer.Render(data, writer);
        var result = writer.ToString();

        // Assert
        result.Should().Contain("\"logicalName\"");
        result.Should().Contain("\"account\"");
        result.Should().Contain("\"relationships\"");
    }

    [Fact]
    public void Render_WithMultipleItems_ProducesJsonArray()
    {
        // Arrange
        var renderer = new JsonRenderer();
        var data = new List<QueueInfo>
        {
            new("Queue 1", "Public", true),
            new("Queue 2", "Private", false)
        };
        using var writer = new StringWriter();

        // Act
        renderer.Render(data, writer);
        var result = writer.ToString();

        // Assert
        result.Should().Contain("\"Queue 1\"");
        result.Should().Contain("\"Queue 2\"");
        result.Should().StartWith("[");
        result.Trim().Should().EndWith("]");
    }
}
