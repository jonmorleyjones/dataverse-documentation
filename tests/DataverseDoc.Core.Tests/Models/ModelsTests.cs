using DataverseDoc.Core.Models;
using FluentAssertions;
using Xunit;

namespace DataverseDoc.Core.Tests.Models;

public class ModelsTests
{
    [Fact]
    public void EnvironmentVariable_CreatesCorrectRecord()
    {
        // Arrange & Act
        var envVar = new EnvironmentVariable(
            DisplayName: "Test Variable",
            SchemaName: "test_variable",
            CurrentValue: "current",
            DefaultValue: "default",
            Type: "String");

        // Assert
        envVar.DisplayName.Should().Be("Test Variable");
        envVar.SchemaName.Should().Be("test_variable");
        envVar.CurrentValue.Should().Be("current");
        envVar.DefaultValue.Should().Be("default");
        envVar.Type.Should().Be("String");
    }

    [Fact]
    public void EnvironmentVariable_WithNullValues_CreatesCorrectRecord()
    {
        // Arrange & Act
        var envVar = new EnvironmentVariable(
            DisplayName: "Test Variable",
            SchemaName: "test_variable",
            CurrentValue: null,
            DefaultValue: null,
            Type: "String");

        // Assert
        envVar.CurrentValue.Should().BeNull();
        envVar.DefaultValue.Should().BeNull();
    }

    [Fact]
    public void SecurityRole_CreatesCorrectRecord()
    {
        // Arrange
        var privileges = new Dictionary<string, List<string>>
        {
            { "account", new List<string> { "Create", "Read", "Write" } }
        };

        // Act
        var role = new SecurityRole(
            Name: "Test Role",
            BusinessUnitName: "Root BU",
            PrivilegesByEntity: privileges);

        // Assert
        role.Name.Should().Be("Test Role");
        role.BusinessUnitName.Should().Be("Root BU");
        role.PrivilegesByEntity.Should().ContainKey("account");
        role.PrivilegesByEntity["account"].Should().HaveCount(3);
    }

    [Fact]
    public void QueueInfo_CreatesCorrectRecord()
    {
        // Arrange & Act
        var queue = new QueueInfo(
            Name: "Support Queue",
            Type: "Public",
            EmailEnabled: true);

        // Assert
        queue.Name.Should().Be("Support Queue");
        queue.Type.Should().Be("Public");
        queue.EmailEnabled.Should().BeTrue();
    }

    [Fact]
    public void OptionSetInfo_CreatesCorrectRecord()
    {
        // Arrange
        var options = new List<OptionValue>
        {
            new(1, "Option 1"),
            new(2, "Option 2")
        };

        // Act
        var optionSet = new OptionSetInfo(
            Name: "Test Option Set",
            Type: "Global",
            Options: options);

        // Assert
        optionSet.Name.Should().Be("Test Option Set");
        optionSet.Type.Should().Be("Global");
        optionSet.Options.Should().HaveCount(2);
    }

    [Fact]
    public void OptionValue_CreatesCorrectRecord()
    {
        // Arrange & Act
        var option = new OptionValue(100, "Active");

        // Assert
        option.Value.Should().Be(100);
        option.Label.Should().Be("Active");
    }

    [Fact]
    public void ProcessInfo_CreatesCorrectRecord()
    {
        // Arrange & Act
        var process = new ProcessInfo(
            Name: "Approval Workflow",
            Type: "Workflow",
            Status: "Active");

        // Assert
        process.Name.Should().Be("Approval Workflow");
        process.Type.Should().Be("Workflow");
        process.Status.Should().Be("Active");
    }

    [Fact]
    public void CloudFlowInfo_CreatesCorrectRecord()
    {
        // Arrange & Act
        var flow = new CloudFlowInfo(
            Name: "Send Email Flow",
            State: "Active",
            Owner: "John Doe");

        // Assert
        flow.Name.Should().Be("Send Email Flow");
        flow.State.Should().Be("Active");
        flow.Owner.Should().Be("John Doe");
    }

    [Fact]
    public void CloudFlowInfo_WithNullOwner_CreatesCorrectRecord()
    {
        // Arrange & Act
        var flow = new CloudFlowInfo(
            Name: "Send Email Flow",
            State: "Active",
            Owner: null);

        // Assert
        flow.Owner.Should().BeNull();
    }

    [Fact]
    public void EntityRelationship_CreatesCorrectRecord()
    {
        // Arrange & Act
        var relationship = new EntityRelationship(
            ParentEntity: "account",
            ChildEntity: "contact",
            RelationshipType: "OneToMany",
            RelationshipName: "account_contacts");

        // Assert
        relationship.ParentEntity.Should().Be("account");
        relationship.ChildEntity.Should().Be("contact");
        relationship.RelationshipType.Should().Be("OneToMany");
        relationship.RelationshipName.Should().Be("account_contacts");
    }

    [Fact]
    public void EntityMetadata_CreatesCorrectRecord()
    {
        // Arrange
        var relationships = new List<EntityRelationship>
        {
            new("account", "contact", "OneToMany", "account_contacts")
        };

        // Act
        var metadata = new EntityMetadata(
            LogicalName: "account",
            DisplayName: "Account",
            Relationships: relationships);

        // Assert
        metadata.LogicalName.Should().Be("account");
        metadata.DisplayName.Should().Be("Account");
        metadata.Relationships.Should().HaveCount(1);
    }
}
