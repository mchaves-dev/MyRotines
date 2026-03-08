namespace MyRotines.Application.Routines;

public sealed class SqlRestoreOptions
{
    public string? Server { get; set; }
    public string? Database { get; set; }
    public string? User { get; set; }
    public string? Password { get; set; }
    public bool TrustedConnection { get; set; }
    public bool ReplaceDatabase { get; set; }
    public string? BackupFilePath { get; set; }
}
