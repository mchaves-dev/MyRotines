using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace MyRotines.Application.Services;

public sealed class SqlServerRestoreService(ILogger<SqlServerRestoreService> logger)
{
    public async Task RestoreAsync(
        string server,
        string database,
        string backupFile,
        bool trustedConnection,
        string? user,
        string? password,
        bool replaceDatabase,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(server))
        {
            throw new ArgumentException("SQL Server é obrigatório.", nameof(server));
        }

        if (string.IsNullOrWhiteSpace(database))
        {
            throw new ArgumentException("Database é obrigatório.", nameof(database));
        }

        if (string.IsNullOrWhiteSpace(backupFile) || !File.Exists(backupFile))
        {
            throw new FileNotFoundException("Arquivo .bak não encontrado.", backupFile);
        }

        if (!trustedConnection && (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password)))
        {
            throw new ArgumentException("Para autenticação SQL, informe usuário e senha.");
        }

        var escapedBackup = backupFile.Replace("'", "''");
        var escapedDb = database.Replace("]", "]]", StringComparison.Ordinal);
        var withClause = replaceDatabase ? "REPLACE, RECOVERY, STATS=5" : "RECOVERY, STATS=5";
        var sql = $"RESTORE DATABASE [{escapedDb}] FROM DISK = N'{escapedBackup}' WITH {withClause};";

        var arguments = BuildArguments(server, trustedConnection, user, password, sql);

        logger.LogInformation("Starting SQL restore. Server: {Server}, Database: {Database}, Backup: {Backup}", server, database, backupFile);

        var startInfo = new ProcessStartInfo
        {
            FileName = "sqlcmd",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var stdOut = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stdErr = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var error = string.IsNullOrWhiteSpace(stdErr) ? stdOut : stdErr;
            throw new InvalidOperationException($"Falha ao executar sqlcmd: {error}".Trim());
        }

        logger.LogInformation("SQL restore completed for database {Database}", database);
    }

    private static string BuildArguments(string server, bool trustedConnection, string? user, string? password, string sql)
    {
        var builder = new StringBuilder();
        builder.Append("-S \"").Append(server).Append("\" -d \"master\" -b ");

        if (trustedConnection)
        {
            builder.Append("-E ");
        }
        else
        {
            builder.Append("-U \"").Append(user).Append("\" -P \"").Append(password).Append("\" ");
        }

        builder.Append("-Q \"").Append(sql.Replace("\"", "\"\"", StringComparison.Ordinal)).Append("\"");
        return builder.ToString();
    }
}
