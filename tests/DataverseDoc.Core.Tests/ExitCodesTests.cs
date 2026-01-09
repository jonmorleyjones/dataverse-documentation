using FluentAssertions;
using Xunit;

namespace DataverseDoc.Core.Tests;

public class ExitCodesTests
{
    [Fact]
    public void Success_ShouldBeZero()
    {
        ExitCodes.Success.Should().Be(0);
    }

    [Fact]
    public void GeneralError_ShouldBeOne()
    {
        ExitCodes.GeneralError.Should().Be(1);
    }

    [Fact]
    public void AuthenticationFailed_ShouldBeTwo()
    {
        ExitCodes.AuthenticationFailed.Should().Be(2);
    }

    [Fact]
    public void ConnectionError_ShouldBeThree()
    {
        ExitCodes.ConnectionError.Should().Be(3);
    }

    [Fact]
    public void InvalidArguments_ShouldBeFour()
    {
        ExitCodes.InvalidArguments.Should().Be(4);
    }

    [Fact]
    public void ResourceNotFound_ShouldBeFive()
    {
        ExitCodes.ResourceNotFound.Should().Be(5);
    }

    [Fact]
    public void AccessDenied_ShouldBeSix()
    {
        ExitCodes.AccessDenied.Should().Be(6);
    }

    [Fact]
    public void AllExitCodes_ShouldBeUnique()
    {
        var codes = new[]
        {
            ExitCodes.Success,
            ExitCodes.GeneralError,
            ExitCodes.AuthenticationFailed,
            ExitCodes.ConnectionError,
            ExitCodes.InvalidArguments,
            ExitCodes.ResourceNotFound,
            ExitCodes.AccessDenied
        };

        codes.Should().OnlyHaveUniqueItems();
    }
}
