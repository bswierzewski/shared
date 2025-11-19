using Xunit;

namespace Shared.Infrastructure.Tests.Unit;

public class SampleTest
{
    [Fact]
    public void SampleTest_ShouldPass()
    {
        // Arrange
        var value = 5;

        // Act
        var result = value + 5;

        // Assert
        Assert.Equal(10, result);
    }
}
