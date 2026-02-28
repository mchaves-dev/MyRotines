using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace MyRotines.Application.Services;

public sealed class ExtractionService(ILogger<ExtractionService> _logger)
{
    public Task ExtractAsync(string currentPath, string destinationPath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(currentPath))
        {
            _logger.LogWarning("Extraction failed: current path is empty.");
            return Task.CompletedTask;
        }

        if (!File.Exists(currentPath))
        {
            _logger.LogWarning("Extraction failed: file not found at {Path}", currentPath);
            return Task.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(destinationPath))
        {
            _logger.LogWarning("Extraction failed: destination path is empty.");
            return Task.CompletedTask;
        }

        destinationPath = Path.GetFullPath(destinationPath);
        Directory.CreateDirectory(destinationPath);

        try
        {
            ZipFile.ExtractToDirectory(currentPath, destinationPath, overwriteFiles: true);
            _logger.LogInformation("File extracted from {Source} to {Destination}", currentPath, destinationPath);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Extraction canceled for {Path}", currentPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting file from {Path}", currentPath);
            throw;
        }

        return Task.CompletedTask;
    }
}