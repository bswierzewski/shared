using Shared.EnvFileGenerator.Tests.Unit.Options;
using EnvGen = Shared.EnvFileGenerator.Services.EnvFileGenerator;

namespace Shared.EnvFileGenerator.Tests.Unit.Services;

public class EnvFileGeneratorTests
{
    private readonly EnvGen _generator = new();

    [Fact]
    public void GenerateEnvContentFromTypes_WithSimpleOptions_GeneratesCorrectEnvContent()
    {
        // Arrange
        var optionsTypes = new List<Type> { typeof(SimpleOptions) };

        // Act
        var content = _generator.GenerateEnvContentFromTypes(optionsTypes, includeDescriptions: false);

        // Assert
        content.Should().Contain("SIMPLECONFIG__APIURL=https://api.example.com");
        content.Should().Contain("SIMPLECONFIG__MAXRETRIES=3");
        content.Should().Contain("SIMPLECONFIG__ENABLELOGGING=true");
        content.Should().Contain("SIMPLECONFIG__TIMEOUT=30");
        content.Should().Contain("SIMPLECONFIG__APIKEY=");
    }

    [Fact]
    public void GenerateEnvContentFromTypes_WithNestedOptions_GeneratesNestedEnvVariables()
    {
        // Arrange
        var optionsTypes = new List<Type> { typeof(NestedOptions) };

        // Act
        var content = _generator.GenerateEnvContentFromTypes(optionsTypes, includeDescriptions: false);

        // Assert
        content.Should().Contain("NESTEDCONFIG__BASEURL=");
        content.Should().Contain("NESTEDCONFIG__USER=");
        content.Should().Contain("NESTEDCONFIG__KEYS__KEYA=default-key-a");
        content.Should().Contain("NESTEDCONFIG__KEYS__KEYB=");
        content.Should().Contain("NESTEDCONFIG__KEYS__KEYC=default-key-c");
    }

    [Fact]
    public void GenerateEnvContentFromTypes_WithComplexNestedOptions_GeneratesMultipleLevelsOfNesting()
    {
        // Arrange
        var optionsTypes = new List<Type> { typeof(ComplexNestedOptions) };

        // Act
        var content = _generator.GenerateEnvContentFromTypes(optionsTypes, includeDescriptions: false);

        // Assert
        content.Should().Contain("COMPLEXCONFIG__SERVICENAME=");
        content.Should().Contain("COMPLEXCONFIG__DATABASE__CONNECTIONSTRING=Server=localhost;Database=mydb");
        content.Should().Contain("COMPLEXCONFIG__DATABASE__COMMANDTIMEOUT=30");
        content.Should().Contain("COMPLEXCONFIG__DATABASE__CREDENTIALS__USERNAME=admin");
        content.Should().Contain("COMPLEXCONFIG__DATABASE__CREDENTIALS__PASSWORD=");
        content.Should().Contain("COMPLEXCONFIG__CACHE__PROVIDER=InMemory");
        content.Should().Contain("COMPLEXCONFIG__CACHE__EXPIRATIONMINUTES=60");
        content.Should().Contain("COMPLEXCONFIG__CACHE__SERVERSETTINGS__HOST=localhost");
        content.Should().Contain("COMPLEXCONFIG__CACHE__SERVERSETTINGS__PORT=6379");
    }

    [Fact]
    public void GenerateEnvContentFromTypes_WithDescriptions_IncludesTypeInformation()
    {
        // Arrange
        var optionsTypes = new List<Type> { typeof(SimpleOptions) };

        // Act
        var content = _generator.GenerateEnvContentFromTypes(optionsTypes, includeDescriptions: true);

        // Assert
        content.Should().Contain("# Type: string");
        content.Should().Contain("# Type: int");
        content.Should().Contain("# Type: bool");
        content.Should().Contain("# Type: decimal");
    }

    [Fact]
    public void GenerateEnvContentFromTypes_WithMultipleOptions_GeneratesAllSections()
    {
        // Arrange
        var optionsTypes = new List<Type>
        {
            typeof(SimpleOptions),
            typeof(NestedOptions),
            typeof(ComplexNestedOptions)
        };

        // Act
        var content = _generator.GenerateEnvContentFromTypes(optionsTypes, includeDescriptions: false);

        // Assert - verify all sections are present
        content.Should().Contain("# SimpleConfig");
        content.Should().Contain("# NestedConfig");
        content.Should().Contain("# ComplexConfig");
    }

    [Fact]
    public void GenerateEnvContentFromTypes_EmptyList_ReturnsEmptyString()
    {
        // Arrange
        var optionsTypes = new List<Type>();

        // Act
        var content = _generator.GenerateEnvContentFromTypes(optionsTypes, includeDescriptions: false);

        // Assert
        content.Should().BeEmpty();
    }
}
