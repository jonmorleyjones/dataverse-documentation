using DataverseDoc.Core.Models;
using DataverseDoc.Dataverse.Readers;
using FluentAssertions;
using Moq;
using Xunit;

namespace DataverseDoc.Dataverse.Tests.Readers;

public class ReaderInterfaceTests
{
    [Fact]
    public async Task IEnvironmentVariableReader_CanBeMocked()
    {
        // Arrange
        var mockReader = new Mock<IEnvironmentVariableReader>();
        var expectedVariables = new List<EnvironmentVariable>
        {
            new("Test Var", "test_var", "value", "default", "String")
        };

        mockReader.Setup(r => r.GetEnvironmentVariablesAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVariables);

        // Act
        var result = await mockReader.Object.GetEnvironmentVariablesAsync("TestSolution");

        // Assert
        result.Should().HaveCount(1);
        result[0].DisplayName.Should().Be("Test Var");
    }

    [Fact]
    public async Task ISecurityRoleReader_CanBeMocked()
    {
        // Arrange
        var mockReader = new Mock<ISecurityRoleReader>();
        var expectedRoles = new List<SecurityRole>
        {
            new("Admin", "Root BU", new Dictionary<string, List<string>>())
        };

        mockReader.Setup(r => r.GetSecurityRolesAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRoles);

        // Act
        var result = await mockReader.Object.GetSecurityRolesAsync("TestSolution");

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Admin");
    }

    [Fact]
    public async Task IQueueReader_CanBeMocked()
    {
        // Arrange
        var mockReader = new Mock<IQueueReader>();
        var expectedQueues = new List<QueueInfo>
        {
            new("Support", "Public", true)
        };

        mockReader.Setup(r => r.GetQueuesAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedQueues);

        // Act
        var result = await mockReader.Object.GetQueuesAsync("TestSolution");

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Support");
    }

    [Fact]
    public async Task IOptionSetReader_CanBeMocked()
    {
        // Arrange
        var mockReader = new Mock<IOptionSetReader>();
        var expectedOptionSets = new List<OptionSetInfo>
        {
            new("Status", "Global", new List<OptionValue> { new(1, "Active") })
        };

        mockReader.Setup(r => r.GetOptionSetsAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOptionSets);

        // Act
        var result = await mockReader.Object.GetOptionSetsAsync("TestSolution");

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Status");
    }

    [Fact]
    public async Task IProcessReader_CanBeMocked()
    {
        // Arrange
        var mockReader = new Mock<IProcessReader>();
        var expectedProcesses = new List<ProcessInfo>
        {
            new("Approval", "Workflow", "Active")
        };

        mockReader.Setup(r => r.GetProcessesAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProcesses);

        // Act
        var result = await mockReader.Object.GetProcessesAsync("TestSolution");

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Approval");
    }

    [Fact]
    public async Task ICloudFlowReader_CanBeMocked()
    {
        // Arrange
        var mockReader = new Mock<ICloudFlowReader>();
        var expectedFlows = new List<CloudFlowInfo>
        {
            new("Email Flow", "Active", "John Doe")
        };

        mockReader.Setup(r => r.GetCloudFlowsAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFlows);

        // Act
        var result = await mockReader.Object.GetCloudFlowsAsync("TestSolution");

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Email Flow");
    }

    [Fact]
    public async Task IEntityMetadataReader_CanBeMocked()
    {
        // Arrange
        var mockReader = new Mock<IEntityMetadataReader>();
        var expectedMetadata = new EntityMetadata(
            "account",
            "Account",
            new List<EntityRelationship>
            {
                new("account", "contact", "OneToMany", "account_contacts")
            });

        mockReader.Setup(r => r.GetEntityMetadataAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedMetadata);

        // Act
        var result = await mockReader.Object.GetEntityMetadataAsync("account", 1);

        // Assert
        result.LogicalName.Should().Be("account");
        result.Relationships.Should().HaveCount(1);
    }
}
