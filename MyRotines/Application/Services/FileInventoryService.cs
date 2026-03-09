namespace MyRotines.Application.Services;

public sealed record FileExtensionSummary(string Extension, int Count, long TotalBytes);

public sealed class FileInventoryService
{
    public IReadOnlyCollection<FileExtensionSummary> SummarizeByExtension(string rootPath, bool recursive)
    {
        if (!Directory.Exists(rootPath))
        {
            throw new DirectoryNotFoundException($"Diretório não encontrado: {rootPath}");
        }

        var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        var summaries = Directory
            .EnumerateFiles(rootPath, "*", option)
            .Select(path => new FileInfo(path))
            .GroupBy(info => string.IsNullOrWhiteSpace(info.Extension) ? "(sem extensão)" : info.Extension.ToLowerInvariant())
            .Select(group => new FileExtensionSummary(
                group.Key,
                group.Count(),
                group.Sum(file => file.Length)))
            .OrderByDescending(summary => summary.TotalBytes)
            .ToArray();

        return summaries;
    }
}
