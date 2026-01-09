using DataverseDoc.Core.Models;
using DataverseDoc.Renderers;
using FluentAssertions;
using Xunit;

namespace DataverseDoc.Core.Tests.Renderers;

public class MarkdownRendererTests
{
    [Fact]
    public void Render_WithEnvironmentVariables_ProducesMarkdownTable()
    {
        // Arrange
        var renderer = new MarkdownRenderer();
        var data = new List<EnvironmentVariable>
        {
            new("Test Variable", "test_var", "current", "default", "String")
        };
        using var writer = new StringWriter();

        // Act
        renderer.Render(data, writer);
        var result = writer.ToString();

        // Assert
        result.Should().Contain("| Display Name |");
        result.Should().Contain("| --- |");
        result.Should().Contain("| Test Variable |");
    }

    [Fact]
    public void Render_WithEmptyList_ProducesNoDataMessage()
    {
        // Arrange
        var renderer = new MarkdownRenderer();
        var data = new List<EnvironmentVariable>();
        using var writer = new StringWriter();

        // Act
        renderer.Render(data, writer);
        var result = writer.ToString();

        // Assert
        result.Should().Contain("No data found");
    }

    [Fact]
    public void Render_WithBooleanValues_FormatsCorrectly()
    {
        // Arrange
        var renderer = new MarkdownRenderer();
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
        result.Should().Contain("| Yes |");
        result.Should().Contain("| No |");
    }

    [Fact]
    public void Render_WithNullValues_ShowsDash()
    {
        // Arrange
        var renderer = new MarkdownRenderer();
        var data = new List<EnvironmentVariable>
        {
            new("Test Variable", "test_var", null, null, "String")
        };
        using var writer = new StringWriter();

        // Act
        renderer.Render(data, writer);
        var result = writer.ToString();

        // Assert
        result.Should().Contain("| - |");
    }

    [Fact]
    public void Render_EscapesPipeCharacters()
    {
        // Arrange
        var renderer = new MarkdownRenderer();
        var data = new List<EnvironmentVariable>
        {
            new("Test|Variable", "test_var", "value|with|pipes", null, "String")
        };
        using var writer = new StringWriter();

        // Act
        renderer.Render(data, writer);
        var result = writer.ToString();

        // Assert
        result.Should().Contain("\\|");
    }
}
