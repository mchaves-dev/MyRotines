using MyRotines.Application.Services;

namespace MyRotines.Application.Routines.Steps;

public sealed class RestoreSqlRoutineStep(SqlServerRestoreService restoreService) : IRoutineStep
{
    public string Key => "restore-sql";
    public string Description => "Restaura backup .bak no SQL Server";

    public async Task ExecuteAsync(RoutineExecutionContext context, CancellationToken cancellationToken)
    {
        var options = context.SqlRestore;

        if (string.IsNullOrWhiteSpace(options.Server) || string.IsNullOrWhiteSpace(options.Database))
        {
            throw new InvalidOperationException("Step 'restore-sql' exige --sql-server e --sql-db.");
        }

        var backupFile = context.ResolveBackupFilePath();

        await restoreService.RestoreAsync(
            server: options.Server,
            database: options.Database,
            backupFile: backupFile,
            trustedConnection: options.TrustedConnection,
            user: options.User,
            password: options.Password,
            replaceDatabase: options.ReplaceDatabase,
            cancellationToken: cancellationToken);
    }
}
