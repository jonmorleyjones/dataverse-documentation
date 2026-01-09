using System.CommandLine;
using System.CommandLine.Invocation;
using FluentAssertions;
using Xunit;

namespace DataverseDoc.Integration.Tests;

public class CliTests
{
    [Fact]
    public async Task RootCommand_WithHelpOption_ReturnsZero()
    {
        // Arrange
        var args = new[] { "--help" };

        // Act
        var exitCode = await DataverseDoc.Cli.Program.Main(args);

        // Assert
        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task EnvvarsCommand_WithHelpOption_ReturnsZero()
    {
        // Arrange
        var args = new[] { "envvars", "--help" };

        // Act
        var exitCode = await DataverseDoc.Cli.Program.Main(args);

        // Assert
        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task RolesCommand_WithHelpOption_ReturnsZero()
    {
        // Arrange
        var args = new[] { "roles", "--help" };

        // Act
        var exitCode = await DataverseDoc.Cli.Program.Main(args);

        // Assert
        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task QueuesCommand_WithHelpOption_ReturnsZero()
    {
        // Arrange
        var args = new[] { "queues", "--help" };

        // Act
        var exitCode = await DataverseDoc.Cli.Program.Main(args);

        // Assert
        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task EntityDiagramCommand_WithHelpOption_ReturnsZero()
    {
        // Arrange
        var args = new[] { "entity-diagram", "--help" };

        // Act
        var exitCode = await DataverseDoc.Cli.Program.Main(args);

        // Assert
        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task OptionSetsCommand_WithHelpOption_ReturnsZero()
    {
        // Arrange
        var args = new[] { "optionsets", "--help" };

        // Act
        var exitCode = await DataverseDoc.Cli.Program.Main(args);

        // Assert
        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task ProcessesCommand_WithHelpOption_ReturnsZero()
    {
        // Arrange
        var args = new[] { "processes", "--help" };

        // Act
        var exitCode = await DataverseDoc.Cli.Program.Main(args);

        // Assert
        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task FlowsCommand_WithHelpOption_ReturnsZero()
    {
        // Arrange
        var args = new[] { "flows", "--help" };

        // Act
        var exitCode = await DataverseDoc.Cli.Program.Main(args);

        // Assert
        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task FlowDiagramCommand_WithHelpOption_ReturnsZero()
    {
        // Arrange
        var args = new[] { "flow-diagram", "--help" };

        // Act
        var exitCode = await DataverseDoc.Cli.Program.Main(args);

        // Assert
        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task InvalidCommand_ReturnsNonZero()
    {
        // Arrange
        var args = new[] { "invalid-command" };

        // Act
        var exitCode = await DataverseDoc.Cli.Program.Main(args);

        // Assert
        exitCode.Should().NotBe(0);
    }

    [Fact]
    public async Task EnvvarsCommand_WithoutSolution_ReturnsError()
    {
        // Arrange
        var args = new[] { "envvars" };

        // Act
        var exitCode = await DataverseDoc.Cli.Program.Main(args);

        // Assert
        exitCode.Should().NotBe(0);
    }
}
