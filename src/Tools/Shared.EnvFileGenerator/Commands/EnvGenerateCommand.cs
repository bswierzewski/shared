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

        var pathOption = new Option<string>("--path", "Path to the project to scan (scans bin folder)")
        {
            DefaultValueFactory = _ => "."
        };
        pathOption.Aliases.Add("-p");

        var outputOption = new Option<string>("--output", "Output .env file path")
        {
            DefaultValueFactory = _ => ".env.example"
        };
        outputOption.Aliases.Add("-o");

        var recursiveOption = new Option<bool>("--recursive", "Recursively scan referenced assemblies")
        {
            DefaultValueFactory = _ => true
        };
        recursiveOption.Aliases.Add("-r");

        var includeDescriptionsOption = new Option<bool>("--descriptions", "Include descriptions as comments in the output")
        {
            DefaultValueFactory = _ => true
        };
        includeDescriptionsOption.Aliases.Add("-d");

        var overwriteOption = new Option<bool>("--force", "Overwrite existing file without prompting")
        {
            DefaultValueFactory = _ => false
        };
        overwriteOption.Aliases.Add("-f");

        var configurationOption = new Option<string>("--config", "Build configuration (Debug|Release)")
        {
            DefaultValueFactory = _ => "Debug"
        };
        configurationOption.Aliases.Add("-c");

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
