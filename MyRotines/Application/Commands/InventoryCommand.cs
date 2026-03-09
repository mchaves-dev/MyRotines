using MyRotines.Application.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace MyRotines.Application.Commands;

public sealed class InventoryCommand(FileInventoryService inventoryService) : Command<InventoryCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("--path <PATH>")]
        [Description("Diretório para inventário")]
        public string Path { get; set; } = Directory.GetCurrentDirectory();

        [CommandOption("--recursive")]
        [Description("Inventário recursivo")]
        public bool Recursive { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var path = System.IO.Path.GetFullPath(settings.Path);
        var summaries = inventoryService.SummarizeByExtension(path, settings.Recursive);

        var table = new Table().Border(TableBorder.Rounded).AddColumns("Extensão", "Arquivos", "Total (MB)");

        foreach (var summary in summaries)
        {
            table.AddRow(summary.Extension, summary.Count.ToString(), (summary.TotalBytes / 1024d / 1024d).ToString("N2"));
        }

        AnsiConsole.Write(new Panel(table).Header("[bold cyan]Inventário de Arquivos[/]").Expand());
        return 0;
    }
}
