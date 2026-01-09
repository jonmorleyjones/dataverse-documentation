using System.Text.Json;
using DataverseDoc.Core.Models;
using DataverseDoc.Dataverse.Readers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DataverseDoc.Dataverse.Tests.Readers;

public class EnvironmentVariableReaderTests
{
    private readonly Mock<ILogger<EnvironmentVariableReader>> _loggerMock;
    private readonly Mock<IDataverseClient> _clientMock;

    public EnvironmentVariableReaderTests()
    {
        _loggerMock = new Mock<ILogger<EnvironmentVariableReader>>();
        _clientMock = new Mock<IDataverseClient>();
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new EnvironmentVariableReader(null!, _clientMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithNullClient_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new EnvironmentVariableReader(_loggerMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("dataverseClient");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetEnvironmentVariablesAsync_WithInvalidSolutionName_ThrowsArgumentException(string? solutionName)
    {
        // Arrange
        var reader = new EnvironmentVariableReader(_loggerMock.Object, _clientMock.Object);

        // Act
        var act = async () => await reader.GetEnvironmentVariablesAsync(solutionName!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("solutionName");
    }

    [Fact]
    public async Task GetEnvironmentVariablesAsync_ReturnsEnvironmentVariables()
    {
        // Arrange
        var solutionResponse = CreateSolutionResponse("12345678-1234-1234-1234-123456789012");
        var envVarResponse = CreateEnvironmentVariablesResponse();

        SetupClientMock(solutionResponse, envVarResponse);

        var reader = new EnvironmentVariableReader(_loggerMock.Object, _clientMock.Object);

        // Act
        var result = await reader.GetEnvironmentVariablesAsync("TestSolution");

        // Assert
        result.Should().HaveCount(2);
        result[0].DisplayName.Should().Be("API Endpoint");
        result[0].SchemaName.Should().Be("contoso_apiendpoint");
        result[0].Type.Should().Be("String");
        result[0].DefaultValue.Should().Be("https://api.example.com");
        result[0].CurrentValue.Should().Be("https://api.prod.example.com");
    }

    [Fact]
    public async Task GetEnvironmentVariablesAsync_WithNoCurrentValue_ReturnsNullCurrentValue()
    {
        // Arrange
        var solutionResponse = CreateSolutionResponse("12345678-1234-1234-1234-123456789012");
        var envVarResponse = CreateEnvironmentVariablesResponseWithNoCurrentValue();

        SetupClientMock(solutionResponse, envVarResponse);

        var reader = new EnvironmentVariableReader(_loggerMock.Object, _clientMock.Object);

        // Act
        var result = await reader.GetEnvironmentVariablesAsync("TestSolution");

        // Assert
        result.Should().HaveCount(1);
        result[0].CurrentValue.Should().BeNull();
        result[0].DefaultValue.Should().Be("default-value");
    }

    [Fact]
    public async Task GetEnvironmentVariablesAsync_WithEmptySolution_ReturnsEmptyList()
    {
        // Arrange
        var solutionResponse = CreateSolutionResponse("12345678-1234-1234-1234-123456789012");
        var envVarResponse = CreateEmptyResponse();

        SetupClientMock(solutionResponse, envVarResponse);

        var reader = new EnvironmentVariableReader(_loggerMock.Object, _clientMock.Object);

        // Act
        var result = await reader.GetEnvironmentVariablesAsync("EmptySolution");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEnvironmentVariablesAsync_SolutionNotFound_ThrowsDataverseException()
    {
        // Arrange
        var emptyResponse = CreateEmptyResponse();
        _clientMock.Setup(c => c.ExecuteWebApiAsync(
                It.Is<string>(s => s.Contains("solutions")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonDocument.Parse(emptyResponse));

        var reader = new EnvironmentVariableReader(_loggerMock.Object, _clientMock.Object);

        // Act
        var act = async () => await reader.GetEnvironmentVariablesAsync("NonExistentSolution");

        // Assert
        await act.Should().ThrowAsync<DataverseException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task GetEnvironmentVariablesAsync_MapsTypeCorrectly()
    {
        // Arrange
        var solutionResponse = CreateSolutionResponse("12345678-1234-1234-1234-123456789012");
        var envVarResponse = CreateEnvironmentVariablesWithAllTypes();

        SetupClientMock(solutionResponse, envVarResponse);

        var reader = new EnvironmentVariableReader(_loggerMock.Object, _clientMock.Object);

        // Act
        var result = await reader.GetEnvironmentVariablesAsync("TestSolution");

        // Assert
        result.Should().Contain(v => v.Type == "String");
        result.Should().Contain(v => v.Type == "Number");
        result.Should().Contain(v => v.Type == "Boolean");
        result.Should().Contain(v => v.Type == "JSON");
    }

    [Fact]
    public async Task GetEnvironmentVariablesAsync_Cancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _clientMock.Setup(c => c.ExecuteWebApiAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var reader = new EnvironmentVariableReader(_loggerMock.Object, _clientMock.Object);

        // Act
        var act = async () => await reader.GetEnvironmentVariablesAsync("TestSolution", cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    private void SetupClientMock(string solutionResponse, string envVarResponse)
    {
        _clientMock.Setup(c => c.ExecuteWebApiAsync(
                It.Is<string>(s => s.Contains("solutions")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonDocument.Parse(solutionResponse));

        _clientMock.Setup(c => c.ExecuteWebApiAsync(
                It.Is<string>(s => s.Contains("environmentvariabledefinitions")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonDocument.Parse(envVarResponse));
    }

    private static string CreateSolutionResponse(string solutionId)
    {
        return $$"""
        {
            "value": [
                {
                    "solutionid": "{{solutionId}}",
                    "uniquename": "TestSolution",
                    "friendlyname": "Test Solution"
                }
            ]
        }
        """;
    }

    private static string CreateEnvironmentVariablesResponse()
    {
        return """
        {
            "value": [
                {
                    "displayname": "API Endpoint",
                    "schemaname": "contoso_apiendpoint",
                    "type": 100000000,
                    "defaultvalue": "https://api.example.com",
                    "environmentvariabledefinitionid": "11111111-1111-1111-1111-111111111111",
                    "environmentvariabledefinition_environmentvariablevalue": [
                        {
                            "value": "https://api.prod.example.com"
                        }
                    ]
                },
                {
                    "displayname": "Max Retries",
                    "schemaname": "contoso_maxretries",
                    "type": 100000001,
                    "defaultvalue": "3",
                    "environmentvariabledefinitionid": "22222222-2222-2222-2222-222222222222",
                    "environmentvariabledefinition_environmentvariablevalue": [
                        {
                            "value": "5"
                        }
                    ]
                }
            ]
        }
        """;
    }

    private static string CreateEnvironmentVariablesResponseWithNoCurrentValue()
    {
        return """
        {
            "value": [
                {
                    "displayname": "Test Variable",
                    "schemaname": "contoso_testvariable",
                    "type": 100000000,
                    "defaultvalue": "default-value",
                    "environmentvariabledefinitionid": "11111111-1111-1111-1111-111111111111",
                    "environmentvariabledefinition_environmentvariablevalue": []
                }
            ]
        }
        """;
    }

    private static string CreateEnvironmentVariablesWithAllTypes()
    {
        return """
        {
            "value": [
                {
                    "displayname": "String Var",
                    "schemaname": "contoso_stringvar",
                    "type": 100000000,
                    "defaultvalue": "text",
                    "environmentvariabledefinitionid": "11111111-1111-1111-1111-111111111111",
                    "environmentvariabledefinition_environmentvariablevalue": []
                },
                {
                    "displayname": "Number Var",
                    "schemaname": "contoso_numbervar",
                    "type": 100000001,
                    "defaultvalue": "42",
                    "environmentvariabledefinitionid": "22222222-2222-2222-2222-222222222222",
                    "environmentvariabledefinition_environmentvariablevalue": []
                },
                {
                    "displayname": "Boolean Var",
                    "schemaname": "contoso_boolvar",
                    "type": 100000002,
                    "defaultvalue": "true",
                    "environmentvariabledefinitionid": "33333333-3333-3333-3333-333333333333",
                    "environmentvariabledefinition_environmentvariablevalue": []
                },
                {
                    "displayname": "JSON Var",
                    "schemaname": "contoso_jsonvar",
                    "type": 100000003,
                    "defaultvalue": "{}",
                    "environmentvariabledefinitionid": "44444444-4444-4444-4444-444444444444",
                    "environmentvariabledefinition_environmentvariablevalue": []
                }
            ]
        }
        """;
    }

    private static string CreateEmptyResponse()
    {
        return """{"value": []}""";
    }
}
