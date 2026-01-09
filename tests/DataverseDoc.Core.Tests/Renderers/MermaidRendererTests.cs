using DataverseDoc.Core.Models;
using DataverseDoc.Renderers;
using FluentAssertions;
using Xunit;

namespace DataverseDoc.Core.Tests.Renderers;

public class MermaidRendererTests
{
    [Fact]
    public void RenderEntityDiagram_WithOneToManyRelationship_ProducesCorrectSyntax()
    {
        // Arrange
        var entity = new EntityMetadata(
            "account",
            "Account",
            new List<EntityRelationship>
            {
                new("account", "contact", "OneToMany", "has")
            });

        // Act
        var result = MermaidRenderer.RenderEntityDiagram(entity);

        // Assert
        result.Should().Contain("erDiagram");
        result.Should().Contain("ACCOUNT ||--o{ CONTACT");
    }

    [Fact]
    public void RenderEntityDiagram_WithManyToOneRelationship_ProducesCorrectSyntax()
    {
        // Arrange
        var entity = new EntityMetadata(
            "contact",
            "Contact",
            new List<EntityRelationship>
            {
                new("contact", "account", "ManyToOne", "belongs to")
            });

        // Act
        var result = MermaidRenderer.RenderEntityDiagram(entity);

        // Assert
        result.Should().Contain("}o--||");
    }

    [Fact]
    public void RenderEntityDiagram_WithManyToManyRelationship_ProducesCorrectSyntax()
    {
        // Arrange
        var entity = new EntityMetadata(
            "account",
            "Account",
            new List<EntityRelationship>
            {
                new("account", "contact", "ManyToMany", "associated with")
            });

        // Act
        var result = MermaidRenderer.RenderEntityDiagram(entity);

        // Assert
        result.Should().Contain("}o--o{");
    }

    [Fact]
    public void RenderEntityDiagram_WithMultipleRelationships_IncludesAll()
    {
        // Arrange
        var entity = new EntityMetadata(
            "account",
            "Account",
            new List<EntityRelationship>
            {
                new("account", "contact", "OneToMany", "has contacts"),
                new("account", "opportunity", "OneToMany", "has opportunities")
            });

        // Act
        var result = MermaidRenderer.RenderEntityDiagram(entity);

        // Assert
        result.Should().Contain("ACCOUNT");
        result.Should().Contain("CONTACT");
        result.Should().Contain("OPPORTUNITY");
    }

    [Fact]
    public void RenderEntityDiagram_DeduplicatesRelationships()
    {
        // Arrange
        var entity = new EntityMetadata(
            "account",
            "Account",
            new List<EntityRelationship>
            {
                new("account", "contact", "OneToMany", "has"),
                new("account", "contact", "OneToMany", "has")
            });

        // Act
        var result = MermaidRenderer.RenderEntityDiagram(entity);

        // Assert
        var contactCount = result.Split("CONTACT").Length - 1;
        contactCount.Should().Be(1);
    }

    [Fact]
    public void RenderFlowDiagram_WithDependencies_ProducesCorrectSyntax()
    {
        // Arrange
        var dependencies = new List<(string Source, string Target)>
        {
            ("Parent Flow", "Child Flow 1"),
            ("Parent Flow", "Child Flow 2")
        };

        // Act
        var result = MermaidRenderer.RenderFlowDiagram(dependencies);

        // Assert
        result.Should().Contain("flowchart TD");
        result.Should().Contain("Parent_Flow");
        result.Should().Contain("Child_Flow_1");
        result.Should().Contain("-->");
    }

    [Fact]
    public void Render_WithUnsupportedType_ThrowsNotSupportedException()
    {
        // Arrange
        var renderer = new MermaidRenderer();
        using var writer = new StringWriter();

        // Act
        var act = () => renderer.Render("unsupported string", writer);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }
}
