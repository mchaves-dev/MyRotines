namespace MyRotines.Application.Routines;

public sealed class RoutineExecutionContext
{
    public RoutineExecutionContext(
        string url,
        string workDirectory,
        string? fileName,
        string? moveToDirectory,
        bool cleanupDownloadedFile,
        SqlRestoreOptions sqlRestore)
    {
        Url = url;
        WorkDirectory = Path.GetFullPath(workDirectory);
        MoveToDirectory = moveToDirectory;
        CleanupDownloadedFile = cleanupDownloadedFile;
        SqlRestore = sqlRestore;

        var resolvedFileName = ResolveFileName(url, fileName);
        OutputPath = Path.Combine(WorkDirectory, resolvedFileName);
    }

    public string Url { get; }
    public string WorkDirectory { get; }
    public string OutputPath { get; }
    public string? MoveToDirectory { get; }
    public bool CleanupDownloadedFile { get; }
    public SqlRestoreOptions SqlRestore { get; }

    public string? ExtractedDirectoryPath { get; set; }
    public string? FinalPath { get; set; }

    public string ResolvePrimarySourcePath()
    {
        if (!string.IsNullOrWhiteSpace(ExtractedDirectoryPath) && Directory.Exists(ExtractedDirectoryPath))
        {
            return ExtractedDirectoryPath;
        }

        return OutputPath;
    }

    public string ResolveBackupFilePath()
    {
        if (!string.IsNullOrWhiteSpace(SqlRestore.BackupFilePath))
        {
            var explicitPath = Path.GetFullPath(SqlRestore.BackupFilePath);
            if (!File.Exists(explicitPath))
            {
                throw new FileNotFoundException($"Arquivo de backup não encontrado: {explicitPath}");
            }

            return explicitPath;
        }

        var sourcePath = ResolvePrimarySourcePath();

        if (File.Exists(sourcePath) && sourcePath.EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
        {
            return sourcePath;
        }

        if (Directory.Exists(sourcePath))
        {
            var backup = Directory
                .EnumerateFiles(sourcePath, "*.bak", SearchOption.AllDirectories)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(backup))
            {
                return backup;
            }
        }

        throw new FileNotFoundException("Nenhum arquivo .bak foi encontrado para restauração SQL.");
    }

    private static string ResolveFileName(string url, string? explicitFileName)
    {
        if (!string.IsNullOrWhiteSpace(explicitFileName))
        {
            return explicitFileName;
        }

        var uri = new Uri(url);
        var fromUrl = Path.GetFileName(uri.LocalPath);

        return string.IsNullOrWhiteSpace(fromUrl)
            ? $"download-{DateTime.Now:yyyyMMdd-HHmmss}.zip"
            : fromUrl;
    }
}
