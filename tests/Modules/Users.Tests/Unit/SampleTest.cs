using Xunit;

namespace Shared.Users.Tests.Unit;

public class SampleTest
{
    [Fact]
    public void SampleTest_ShouldPass()
    {
        // Arrange
        var value = 10;

        // Act
        var result = value * 2;

        // Assert
        Assert.Equal(20, result);
    }
}
