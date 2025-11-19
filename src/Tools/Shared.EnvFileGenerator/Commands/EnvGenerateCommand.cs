using System.CommandLine;
using System.CommandLine.Parsing;

namespace Shared.EnvFileGenerator.Commands;

/// <summary>
/// Command for generating .env files from IOptions implementations.
/// Scans assemblies to find all configuration classes that implement IOptions,
/// then generates environment variable definitions.
/// </summary>
public static class EnvGenerateCommand
{
    /// <summary>
    /// Creates the 'env generate' command with all its options.
    /// </summary>
    public static Command Create()
    {
        var command = new Command("generate", "Generate .env file from IOptions implementations");

        var pathOption = new Option<string>("--path")
        {
            Aliases = { "-p" },
            Description = "Path to the project to scan (scans bin folder)",
            DefaultValueFactory = _ => "."
        };

        var outputOption = new Option<string>("--output")
        {
            Aliases = { "-o" },
            Description = "Output .env file path",
            DefaultValueFactory = _ => ".env.example"
        };

        var recursiveOption = new Option<bool>("--recursive")
        {
            Aliases = { "-r" },
            Description = "Recursively scan referenced assemblies",
            DefaultValueFactory = _ => true
        };

        var includeDescriptionsOption = new Option<bool>("--descriptions")
        {
            Aliases = { "-d" },
            Description = "Include descriptions as comments in the output",
            DefaultValueFactory = _ => true
        };

        var overwriteOption = new Option<bool>("--force")
        {
            Aliases = { "-f" },
            Description = "Overwrite existing file without prompting",
            DefaultValueFactory = _ => false
        };

        var configurationOption = new Option<string>("--config")
        {
            Aliases = { "-c" },
            Description = "Build configuration (Debug|Release)",
            DefaultValueFactory = _ => "Debug"
        };

        command.Add(pathOption);
        command.Add(outputOption);
        command.Add(recursiveOption);
        command.Add(includeDescriptionsOption);
        command.Add(overwriteOption);
        command.Add(configurationOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var path = parseResult.GetValue(pathOption)!;
            var output = parseResult.GetValue(outputOption)!;
            var recursive = parseResult.GetValue(recursiveOption);
            var includeDescriptions = parseResult.GetValue(includeDescriptionsOption);
            var overwrite = parseResult.GetValue(overwriteOption);
            var config = parseResult.GetValue(configurationOption)!;

            var generator = new Services.EnvFileGenerator();
            await generator.GenerateAsync(
                path,
                output,
                recursive,
                includeDescriptions,
                overwrite,
                config,
                cancellationToken);

            return 0;
        });

        return command;
    }
}
