using Microsoft.Extensions.Logging;

namespace MyRotines.Application.Services;

public sealed class ArquiveDownloadService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ArquiveDownloadService> _logger;

    public ArquiveDownloadService(IHttpClientFactory factory, ILogger<ArquiveDownloadService> logger)
    {
        _logger = logger;
        _httpClient = factory.CreateClient();
    }

    public async Task DownloadAsync(string url, string destinationPath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            _logger.LogWarning("Failed to download from url is empty.");
            return;
        }

        HttpResponseMessage? response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to download from {Url}. Status code: {StatusCode}", url, response.StatusCode);
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

        await using var httpStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        await using var fileStream = new FileStream(
            destinationPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await httpStream.CopyToAsync(fileStream, cancellationToken);
    }
}