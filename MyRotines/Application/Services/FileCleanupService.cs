namespace MyRotines.Application.Services;

public sealed record CleanupFileResult(string Path, bool Deleted);
public sealed record CleanupRunResult(int TotalMatched, int TotalDeleted, long TotalFreedBytes, IReadOnlyCollection<CleanupFileResult> Items);

public sealed class FileCleanupService
{
    public CleanupRunResult CleanupOldFiles(
        string rootPath,
        int olderThanDays,
        string searchPattern,
        bool recursive,
        bool dryRun)
    {
        if (!Directory.Exists(rootPath))
        {
            throw new DirectoryNotFoundException($"Diretório não encontrado: {rootPath}");
        }

        var threshold = DateTime.Now.AddDays(-olderThanDays);
        var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        var matched = Directory
            .EnumerateFiles(rootPath, searchPattern, option)
            .Select(path => new FileInfo(path))
            .Where(info => info.LastWriteTime <= threshold)
            .ToArray();

        var totalFreed = 0L;
        var totalDeleted = 0;
        var items = new List<CleanupFileResult>(matched.Length);

        foreach (var file in matched)
        {
            var deleted = false;

            if (!dryRun)
            {
                var fileSize = file.Length;
                file.Delete();
                totalFreed += fileSize;
                totalDeleted++;
                deleted = true;
            }

            items.Add(new CleanupFileResult(file.FullName, deleted));
        }

        return new CleanupRunResult(
            TotalMatched: matched.Length,
            TotalDeleted: totalDeleted,
            TotalFreedBytes: totalFreed,
            Items: items);
    }
}
