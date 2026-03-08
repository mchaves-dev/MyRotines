using Microsoft.Extensions.Logging;

namespace MyRotines.Application.Services;

public sealed class FileMoveService(ILogger<FileMoveService> logger)
{
    public Task<string> MoveAsync(
        string sourcePath,
        string destinationDirectory,
        bool overwrite,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            throw new ArgumentException("Source path is required.", nameof(sourcePath));
        }

        if (string.IsNullOrWhiteSpace(destinationDirectory))
        {
            throw new ArgumentException("Destination directory is required.", nameof(destinationDirectory));
        }

        var sourceFullPath = Path.GetFullPath(sourcePath);
        var destinationFullPath = Path.GetFullPath(destinationDirectory);

        var sourceIsFile = File.Exists(sourceFullPath);
        var sourceIsDirectory = Directory.Exists(sourceFullPath);

        if (!sourceIsFile && !sourceIsDirectory)
        {
            throw new FileNotFoundException($"Source path was not found: {sourceFullPath}");
        }

        Directory.CreateDirectory(destinationFullPath);

        var name = sourceIsFile
            ? Path.GetFileName(sourceFullPath)
            : new DirectoryInfo(sourceFullPath).Name;

        var targetPath = Path.Combine(destinationFullPath, name);

        if (sourceIsFile)
        {
            File.Move(sourceFullPath, targetPath, overwrite);
        }
        else
        {
            if (Directory.Exists(targetPath))
            {
                if (!overwrite)
                {
                    throw new IOException($"Destination already exists: {targetPath}");
                }

                Directory.Delete(targetPath, recursive: true);
            }

            Directory.Move(sourceFullPath, targetPath);
        }

        logger.LogInformation("Moved path from {Source} to {Target}", sourceFullPath, targetPath);

        return Task.FromResult(targetPath);
    }
}
