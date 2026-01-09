using System.Text.Json;
using DataverseDoc.Core.Models;
using DataverseDoc.Dataverse.Readers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DataverseDoc.Dataverse.Tests.Readers;

public class QueueReaderTests
{
    private readonly Mock<ILogger<QueueReader>> _loggerMock;
    private readonly Mock<IDataverseClient> _clientMock;

    public QueueReaderTests()
    {
        _loggerMock = new Mock<ILogger<QueueReader>>();
        _clientMock = new Mock<IDataverseClient>();
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new QueueReader(null!, _clientMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithNullClient_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new QueueReader(_loggerMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("dataverseClient");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetQueuesAsync_WithInvalidSolutionName_ThrowsArgumentException(string? solutionName)
    {
        // Arrange
        var reader = new QueueReader(_loggerMock.Object, _clientMock.Object);

        // Act
        var act = async () => await reader.GetQueuesAsync(solutionName!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("solutionName");
    }

    [Fact]
    public async Task GetQueuesAsync_ReturnsQueues()
    {
        // Arrange
        var solutionResponse = CreateSolutionResponse("12345678-1234-1234-1234-123456789012");
        var queueResponse = CreateQueuesResponse();

        SetupClientMock(solutionResponse, queueResponse);

        var reader = new QueueReader(_loggerMock.Object, _clientMock.Object);

        // Act
        var result = await reader.GetQueuesAsync("TestSolution");

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Support Queue");
        result[0].Type.Should().Be("Public");
        result[0].EmailEnabled.Should().BeTrue();

        result[1].Name.Should().Be("Sales Queue");
        result[1].Type.Should().Be("Private");
        result[1].EmailEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task GetQueuesAsync_WithEmptySolution_ReturnsEmptyList()
    {
        // Arrange
        var solutionResponse = CreateSolutionResponse("12345678-1234-1234-1234-123456789012");
        var queueResponse = CreateEmptyResponse();

        SetupClientMock(solutionResponse, queueResponse);

        var reader = new QueueReader(_loggerMock.Object, _clientMock.Object);

        // Act
        var result = await reader.GetQueuesAsync("EmptySolution");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetQueuesAsync_SolutionNotFound_ThrowsDataverseException()
    {
        // Arrange
        var emptyResponse = CreateEmptyResponse();
        _clientMock.Setup(c => c.ExecuteWebApiAsync(
                It.Is<string>(s => s.Contains("solutions")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonDocument.Parse(emptyResponse));

        var reader = new QueueReader(_loggerMock.Object, _clientMock.Object);

        // Act
        var act = async () => await reader.GetQueuesAsync("NonExistentSolution");

        // Assert
        await act.Should().ThrowAsync<DataverseException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task GetQueuesAsync_MapsQueueTypeCorrectly()
    {
        // Arrange
        var solutionResponse = CreateSolutionResponse("12345678-1234-1234-1234-123456789012");
        var queueResponse = CreateQueuesWithDifferentTypes();

        SetupClientMock(solutionResponse, queueResponse);

        var reader = new QueueReader(_loggerMock.Object, _clientMock.Object);

        // Act
        var result = await reader.GetQueuesAsync("TestSolution");

        // Assert
        result.Should().Contain(q => q.Type == "Public");
        result.Should().Contain(q => q.Type == "Private");
    }

    [Fact]
    public async Task GetQueuesAsync_DetectsEmailEnabled()
    {
        // Arrange
        var solutionResponse = CreateSolutionResponse("12345678-1234-1234-1234-123456789012");
        var queueResponse = CreateQueuesWithEmailSettings();

        SetupClientMock(solutionResponse, queueResponse);

        var reader = new QueueReader(_loggerMock.Object, _clientMock.Object);

        // Act
        var result = await reader.GetQueuesAsync("TestSolution");

        // Assert
        result.Should().Contain(q => q.EmailEnabled == true);
        result.Should().Contain(q => q.EmailEnabled == false);
    }

    [Fact]
    public async Task GetQueuesAsync_Cancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _clientMock.Setup(c => c.ExecuteWebApiAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var reader = new QueueReader(_loggerMock.Object, _clientMock.Object);

        // Act
        var act = async () => await reader.GetQueuesAsync("TestSolution", cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    private void SetupClientMock(string solutionResponse, string queueResponse)
    {
        _clientMock.Setup(c => c.ExecuteWebApiAsync(
                It.Is<string>(s => s.Contains("solutions")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonDocument.Parse(solutionResponse));

        _clientMock.Setup(c => c.ExecuteWebApiAsync(
                It.Is<string>(s => s.Contains("solutioncomponents")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonDocument.Parse(CreateComponentsResponse()));

        _clientMock.Setup(c => c.ExecuteWebApiAsync(
                It.Is<string>(s => s.Contains("queues")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonDocument.Parse(queueResponse));
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

    private static string CreateComponentsResponse()
    {
        return """
        {
            "value": [
                { "objectid": "11111111-1111-1111-1111-111111111111" },
                { "objectid": "22222222-2222-2222-2222-222222222222" }
            ]
        }
        """;
    }

    private static string CreateQueuesResponse()
    {
        return """
        {
            "value": [
                {
                    "queueid": "11111111-1111-1111-1111-111111111111",
                    "name": "Support Queue",
                    "queuetypecode": 1,
                    "emailaddress": "support@example.com"
                },
                {
                    "queueid": "22222222-2222-2222-2222-222222222222",
                    "name": "Sales Queue",
                    "queuetypecode": 2,
                    "emailaddress": null
                }
            ]
        }
        """;
    }

    private static string CreateQueuesWithDifferentTypes()
    {
        return """
        {
            "value": [
                {
                    "queueid": "11111111-1111-1111-1111-111111111111",
                    "name": "Public Queue",
                    "queuetypecode": 1,
                    "emailaddress": null
                },
                {
                    "queueid": "22222222-2222-2222-2222-222222222222",
                    "name": "Private Queue",
                    "queuetypecode": 2,
                    "emailaddress": null
                }
            ]
        }
        """;
    }

    private static string CreateQueuesWithEmailSettings()
    {
        return """
        {
            "value": [
                {
                    "queueid": "11111111-1111-1111-1111-111111111111",
                    "name": "Email Enabled Queue",
                    "queuetypecode": 1,
                    "emailaddress": "queue@example.com"
                },
                {
                    "queueid": "22222222-2222-2222-2222-222222222222",
                    "name": "No Email Queue",
                    "queuetypecode": 1,
                    "emailaddress": null
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
