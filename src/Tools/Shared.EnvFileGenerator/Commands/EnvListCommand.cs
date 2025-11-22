using System.CommandLine;

namespace Shared.EnvFileGenerator.Commands;

/// <summary>
/// Command for listing all available configuration sections from IOptions implementations.
/// Scans assemblies to find all configuration classes that implement IOptions
/// and displays their section names and properties.
/// </summary>
public static class EnvListCommand
{
    /// <summary>
    /// Creates the 'env list' command with all its options.
    /// </summary>
    public static Command Create()
    {
        var command = new Command("list", "List all available configuration sections");

        var pathOption = new Option<string>("--path")
        {
            Aliases = { "-p" },
            Description = "Path to the project to scan (scans bin folder)",
            DefaultValueFactory = _ => "."
        };

        var recursiveOption = new Option<bool>("--recursive")
        {
            Aliases = { "-r" },
            Description = "Recursively scan referenced assemblies",
            DefaultValueFactory = _ => true
        };

        var configurationOption = new Option<string>("--config")
        {
            Aliases = { "-c" },
            Description = "Build configuration (Debug|Release)",
            DefaultValueFactory = _ => "Debug"
        };

        var verboseOption = new Option<bool>("--verbose")
        {
            Aliases = { "-v" },
            Description = "Show detailed information including all properties",
            DefaultValueFactory = _ => false
        };

        command.Add(pathOption);
        command.Add(recursiveOption);
        command.Add(configurationOption);
        command.Add(verboseOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var path = parseResult.GetValue(pathOption)!;
            var recursive = parseResult.GetValue(recursiveOption);
            var config = parseResult.GetValue(configurationOption)!;
            var verbose = parseResult.GetValue(verboseOption);

            var generator = new Services.EnvFileGenerator();
            await generator.ListSectionsAsync(
                path,
                recursive,
                verbose,
                config,
                cancellationToken);

            return 0;
        });

        return command;
    }
}
