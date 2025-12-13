using System.Reflection;

namespace Shared.EnvFileGenerator.Services;

/// <summary>
/// Main service responsible for coordinating .env file generation.
/// Orchestrates scanning, content generation, and file operations.
/// </summary>
public class EnvFileGenerator
{
    private readonly AssemblyScanner _assemblyScanner;
    private readonly PropertyValueFormatter _formatter;
    private readonly EnvContentGenerator _contentGenerator;
    private readonly EnvFileManager _fileManager;

    /// <summary>
    /// Initializes a new instance of the EnvFileGenerator service.
    /// </summary>
    public EnvFileGenerator()
    {
        _assemblyScanner = new AssemblyScanner();
        _formatter = new PropertyValueFormatter();
        _contentGenerator = new EnvContentGenerator(_formatter);
        _fileManager = new EnvFileManager();
    }

    /// <summary>
    /// Generates .env file content from a list of options types.
    /// Useful for testing and programmatic usage.
    /// </summary>
    public string GenerateEnvContentFromTypes(List<Type> optionsTypes, bool includeDescriptions)
    {
        if (optionsTypes.Count == 0) return "";
        return _contentGenerator.Generate(optionsTypes, includeDescriptions);
    }

    /// <summary>
    /// Generates an .env file by scanning the project's bin folder.
    /// </summary>
    public async Task GenerateAsync(
        string projectPath,
        string outputPath,
        bool recursive,
        bool includeDescriptions,
        bool overwrite,
        string config,
        CancellationToken cancellationToken = default)
    {
        var absoluteOutputPath = _fileManager.ResolvePath(projectPath, outputPath);

        if (!_fileManager.CanWriteFile(absoluteOutputPath, overwrite))
            return;

        var optionsTypes = _assemblyScanner.LoadOptionsTypes(projectPath, config, recursive);
        if (optionsTypes.Count == 0) return;

        var content = GenerateEnvContentFromTypes(optionsTypes, includeDescriptions);

        await _fileManager.WriteFileAsync(absoluteOutputPath, content, cancellationToken);
    }

    /// <summary>
    /// Lists all available configuration sections from IOptions implementations.
    /// </summary>
    public async Task ListSectionsAsync(
        string projectPath,
        bool recursive,
        bool verbose,
        string config,
        CancellationToken cancellationToken = default)
    {
        var optionsTypes = _assemblyScanner.LoadOptionsTypes(projectPath, config, recursive);
        if (optionsTypes.Count == 0) return;

        _contentGenerator.ListSections(optionsTypes, verbose);

        await Task.CompletedTask; // Keep signature async compatible
    }

    /// <summary>
    /// Updates an existing .env file with new configuration sections.
    /// </summary>
    public async Task UpdateAsync(
        string projectPath,
        string envFilePath,
        bool recursive,
        bool includeDescriptions,
        bool backup,
        string config,
        CancellationToken cancellationToken = default)
    {
        var optionsTypes = _assemblyScanner.LoadOptionsTypes(projectPath, config, recursive);
        if (optionsTypes.Count == 0) return;

        var absoluteEnvPath = _fileManager.ResolvePath(projectPath, envFilePath);

        var existingValues = new Dictionary<string, string>();
        if (File.Exists(absoluteEnvPath))
        {
            Console.WriteLine($"Reading existing .env file...");
            if (backup)
            {
                _fileManager.CreateBackup(absoluteEnvPath);
            }

            existingValues = await _fileManager.ParseEnvFileAsync(absoluteEnvPath, cancellationToken);
        }

        var newContent = GenerateEnvContentFromTypes(optionsTypes, includeDescriptions);
        var mergedContent = _fileManager.MergeEnvContent(newContent, existingValues);

        _fileManager.EnsureDirectoryExists(absoluteEnvPath);
        await File.WriteAllTextAsync(absoluteEnvPath, mergedContent, cancellationToken);
        Console.WriteLine($"Updated {absoluteEnvPath} successfully.");
    }
}
