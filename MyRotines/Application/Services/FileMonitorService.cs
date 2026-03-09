using System.Threading.Channels;

namespace MyRotines.Application.Services;

public sealed class FileMonitorService
{
    public async Task MonitorAsync(
        string path,
        string filter,
        bool includeSubdirectories,
        Func<string, CancellationToken, Task> onCreated,
        CancellationToken cancellationToken)
    {
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"Diretório não encontrado: {path}");
        }

        var channel = Channel.CreateUnbounded<string>();

        using var watcher = new FileSystemWatcher(path, filter)
        {
            IncludeSubdirectories = includeSubdirectories,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.Size
        };

        watcher.Created += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.FullPath))
            {
                channel.Writer.TryWrite(args.FullPath);
            }
        };

        watcher.EnableRaisingEvents = true;

        using var registration = cancellationToken.Register(() => channel.Writer.TryComplete());

        while (await channel.Reader.WaitToReadAsync(cancellationToken))
        {
            while (channel.Reader.TryRead(out var filePath))
            {
                await onCreated(filePath, cancellationToken);
            }
        }
    }
}
