using MyRotines.Application.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace MyRotines.Application.Commands;

public sealed class RestoreSqlCommand(SqlServerRestoreService restoreService) : AsyncCommand<RestoreSqlCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("--backup <PATH>")]
        [Description("Caminho do arquivo .bak")]
        public string BackupFile { get; set; } = string.Empty;

        [CommandOption("--sql-server <SERVER>")]
        [Description("Servidor SQL, ex.: .\\SQLEXPRESS")]
        public string Server { get; set; } = string.Empty;

        [CommandOption("--sql-db <DB>")]
        [Description("Nome do banco destino")]
        public string Database { get; set; } = string.Empty;

        [CommandOption("--sql-trusted")]
        [Description("Usa autenticação integrada (Windows)")]
        public bool TrustedConnection { get; set; }

        [CommandOption("--sql-user <USER>")]
        [Description("Usuário SQL")]
        public string? User { get; set; }

        [CommandOption("--sql-pass <PASS>")]
        [Description("Senha SQL")]
        public string? Password { get; set; }

        [CommandOption("--sql-replace")]
        [Description("Usa WITH REPLACE no restore")]
        public bool ReplaceDatabase { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.BackupFile) ||
            string.IsNullOrWhiteSpace(settings.Server) ||
            string.IsNullOrWhiteSpace(settings.Database))
        {
            AnsiConsole.MarkupLine("[red]--backup, --sql-server e --sql-db são obrigatórios[/]");
            return -1;
        }

        try
        {
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("Restaurando backup no SQL Server...", async _ =>
                {
                    await restoreService.RestoreAsync(
                        settings.Server,
                        settings.Database,
                        settings.BackupFile,
                        settings.TrustedConnection,
                        settings.User,
                        settings.Password,
                        settings.ReplaceDatabase,
                        cancellationToken);
                });

            AnsiConsole.MarkupLine("[green]Restore SQL concluído com sucesso.[/]");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.Write(new Panel(ex.Message).Header("[red]Erro no Restore SQL[/]").Border(BoxBorder.Rounded));
            return -1;
        }
    }
}
