using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace MyRotines.Application.Services;

public sealed class CompressionService(ILogger<CompressionService> _logger)
{
    public async Task CompressionAsync(
       string sourceDirectory,
       string destinationZipPath,
       CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sourceDirectory))
        {
            _logger.LogWarning("Compression failed: source directory is empty.");
            return;
        }

        if (!Directory.Exists(sourceDirectory))
        {
            _logger.LogWarning("Compression failed: directory not found at {Path}", sourceDirectory);
            return;
        }

        if (string.IsNullOrWhiteSpace(destinationZipPath))
        {
            _logger.LogWarning("Compression failed: destination zip path is empty.");
            return;
        }

        destinationZipPath = Path.GetFullPath(destinationZipPath);

        var destinationDirectory = Path.GetDirectoryName(destinationZipPath);
        if (!string.IsNullOrEmpty(destinationDirectory))
            Directory.CreateDirectory(destinationDirectory);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Run(() =>
            {
                // Remove zip existente para evitar exceção
                if (File.Exists(destinationZipPath))
                    File.Delete(destinationZipPath);

                ZipFile.CreateFromDirectory(
                    sourceDirectory,
                    destinationZipPath,
                    CompressionLevel.Optimal,
                    includeBaseDirectory: true);
            }, cancellationToken);

            _logger.LogInformation(
                "Directory compressed from {Source} to {Destination}",
                sourceDirectory,
                destinationZipPath);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Compression canceled for {Path}", sourceDirectory);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compressing directory from {Path}", sourceDirectory);
            throw;
        }
    }
}