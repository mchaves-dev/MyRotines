using MyRotines.Application.Routines;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MyRotines.Application.Commands;

public class ProfilesCommand(
    RoutineProfilesOptions profilesOptions,
    RoutineStepCatalog stepCatalog) : Command
{
    public override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        var profilesTable = new Table().Border(TableBorder.Rounded).AddColumns("Profile", "Steps");

        foreach (var profile in profilesOptions.Profiles.OrderBy(x => x.Key))
        {
            profilesTable.AddRow(profile.Key, string.Join(" -> ", profile.Value));
        }

        var stepsTable = new Table().Border(TableBorder.Rounded).AddColumns("Step", "Descrição");
        foreach (var step in stepCatalog.GetAll())
        {
            stepsTable.AddRow(step.Key, step.Description);
        }

        AnsiConsole.Write(new Panel(profilesTable).Header("[bold cyan]Perfis Disponíveis[/]").Expand());
        AnsiConsole.Write(new Panel(stepsTable).Header("[bold cyan]Steps Disponíveis[/]").Expand());

        AnsiConsole.MarkupLine("Edite [yellow]appsettings.json[/] para criar seus próprios profiles e comandos de rotina.");

        return 0;
    }
}
