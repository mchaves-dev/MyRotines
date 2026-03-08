using Microsoft.Extensions.Logging;

namespace MyRotines.Application.Services;

public sealed class DownloadService
{
    private static readonly byte[] ZipHeader = [0x50, 0x4B, 0x03, 0x04];

    private readonly HttpClient _httpClient;
    private readonly ILogger<DownloadService> _logger;

    public DownloadService(IHttpClientFactory factory, ILogger<DownloadService> logger)
    {
        _logger = logger;
        _httpClient = factory.CreateClient();
    }

    public async Task DownloadAsync(string url, string destinationPath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL é obrigatória.", nameof(url));
        }

        HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Falha no download. HTTP {(int)response.StatusCode}.");
        }

        var destinationDirectory = Path.GetDirectoryName(destinationPath);

        if (!string.IsNullOrWhiteSpace(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        await using var httpStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var fileStream = new FileStream(
            destinationPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await httpStream.CopyToAsync(fileStream, cancellationToken);
        await fileStream.FlushAsync(cancellationToken);

        var looksLikeZip = await LooksLikeZipAsync(destinationPath, cancellationToken);
        if (!looksLikeZip)
        {
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "desconhecido";
            throw new InvalidDataException(
                $"Arquivo baixado não parece ZIP válido (Content-Type: {contentType}). " +
                "Se a URL tiver '&', execute entre aspas duplas no CMD.");
        }

        _logger.LogInformation("Download concluído: {DestinationPath}", destinationPath);
    }

    private static async Task<bool> LooksLikeZipAsync(string filePath, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4, useAsync: true);

        if (stream.Length < 4)
        {
            return false;
        }

        var header = new byte[4];
        var read = await stream.ReadAsync(header, cancellationToken);

        return read == 4 && header.SequenceEqual(ZipHeader);
    }
}
