using DataverseDoc.Core.Configuration;
using DataverseDoc.Renderers;
using FluentAssertions;
using Xunit;

namespace DataverseDoc.Core.Tests.Renderers;

public class OutputRendererFactoryTests
{
    [Fact]
    public void Create_WithTableFormat_ReturnsTableRenderer()
    {
        // Act
        var renderer = OutputRendererFactory.Create(OutputFormat.Table);

        // Assert
        renderer.Should().BeOfType<TableRenderer>();
    }

    [Fact]
    public void Create_WithJsonFormat_ReturnsJsonRenderer()
    {
        // Act
        var renderer = OutputRendererFactory.Create(OutputFormat.Json);

        // Assert
        renderer.Should().BeOfType<JsonRenderer>();
    }

    [Fact]
    public void Create_WithMarkdownFormat_ReturnsMarkdownRenderer()
    {
        // Act
        var renderer = OutputRendererFactory.Create(OutputFormat.Markdown);

        // Assert
        renderer.Should().BeOfType<MarkdownRenderer>();
    }

    [Fact]
    public void Create_WithMermaidFormat_ThrowsNotSupportedException()
    {
        // Act
        var act = () => OutputRendererFactory.Create(OutputFormat.Mermaid);

        // Assert
        act.Should().Throw<NotSupportedException>()
            .WithMessage("*Mermaid*ISingleOutputRenderer*");
    }

    [Fact]
    public void CreateSingle_WithJsonFormat_ReturnsJsonRenderer()
    {
        // Act
        var renderer = OutputRendererFactory.CreateSingle(OutputFormat.Json);

        // Assert
        renderer.Should().BeOfType<JsonRenderer>();
    }

    [Fact]
    public void CreateSingle_WithMermaidFormat_ReturnsMermaidRenderer()
    {
        // Act
        var renderer = OutputRendererFactory.CreateSingle(OutputFormat.Mermaid);

        // Assert
        renderer.Should().BeOfType<MermaidRenderer>();
    }

    [Fact]
    public void CreateSingle_WithTableFormat_ThrowsNotSupportedException()
    {
        // Act
        var act = () => OutputRendererFactory.CreateSingle(OutputFormat.Table);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void CreateSingle_WithMarkdownFormat_ThrowsNotSupportedException()
    {
        // Act
        var act = () => OutputRendererFactory.CreateSingle(OutputFormat.Markdown);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }
}
