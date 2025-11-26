using Shared.Infrastructure.Tests.Core;

namespace Shared.Infrastructure.Tests.Fixtures;

/// <summary>
/// Interface for xUnit test host fixtures.
/// Fixtures allow sharing expensive resources (like test containers) across multiple test classes.
/// </summary>
public interface ITestHostFixture
{
    /// <summary>
    /// Gets the shared test context.
    /// </summary>
    TestContext Context { get; }
}
