#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Collections.Generic;
using System.Text;

namespace Shared.EnvFileGenerator.Services;

/// <summary>
/// Manages .env file operations (reading, writing, parsing, merging).
/// </summary>
internal class EnvFileManager
{
    /// <summary>
    /// Checks if a file exists and warns if overwrite is not allowed.
    /// </summary>
    public bool CanWriteFile(string filePath, bool overwrite)
    {
        if (File.Exists(filePath) && !overwrite)
        {
            Console.WriteLine($"File {filePath} already exists. Use --force to overwrite.");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Ensures the directory for a file path exists.
    /// </summary>
    public void EnsureDirectoryExists(string filePath)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir!);
    }

    /// <summary>
    /// Resolves a potentially relative path to an absolute path.
    /// </summary>
    public string ResolvePath(string projectPath, string relativePath)
    {
        var absoluteProjectPath = Path.GetFullPath(projectPath);
        return Path.IsPathRooted(relativePath) ? relativePath : Path.Combine(absoluteProjectPath, relativePath);
    }

    /// <summary>
    /// Writes content to a file asynchronously.
    /// </summary>
    public async Task WriteFileAsync(string filePath, string content, CancellationToken cancellationToken = default)
    {
        EnsureDirectoryExists(filePath);
        await File.WriteAllTextAsync(filePath, content, cancellationToken);
        Console.WriteLine($"Generated {filePath} successfully.");
    }

    /// <summary>
    /// Creates a backup of an existing file.
    /// </summary>
    public void CreateBackup(string filePath)
    {
        var backupPath = $"{filePath}.backup";
        File.Copy(filePath, backupPath, overwrite: true);
        Console.WriteLine($"Backup created: {backupPath}");
    }

    /// <summary>
    /// Parses an existing .env file and returns key-value pairs.
    /// </summary>
    public async Task<Dictionary<string, string>> ParseEnvFileAsync(string filePath, CancellationToken ct)
    {
        var result = new Dictionary<string, string>();
        var lines = await File.ReadAllLinesAsync(filePath, ct);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                continue;

            var parts = trimmed.Split('=', 2);
            if (parts.Length == 2)
                result[parts[0]] = parts[1];
        }
        return result;
    }

    /// <summary>
    /// Merges new content with existing values from the file.
    /// </summary>
    public string MergeEnvContent(string newContent, Dictionary<string, string> existingValues)
    {
        var sb = new StringBuilder();
        foreach (var line in newContent.Split(Environment.NewLine))
        {
            var trimmed = line.Trim();
            if (!string.IsNullOrEmpty(trimmed) && !trimmed.StartsWith("#"))
            {
                var parts = trimmed.Split('=', 2);
                if (parts.Length == 2 && existingValues.TryGetValue(parts[0], out var existingVal))
                {
                    sb.AppendLine($"{parts[0]}={existingVal}");
                    continue;
                }
            }
            sb.AppendLine(line);
        }
        return sb.ToString();
    }
}
