using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyRotines.Application;
using MyRotines.Application.Commands;
using MyRotines.Application.Extensions;
using MyRotines.Application.Routines;
using MyRotines.Application.Routines.Steps;
using MyRotines.Application.Services;
using MyRotines.Domain.Events;
using Spectre.Console.Cli;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false)
    .Build();

var routineProfiles = new RoutineProfilesOptions();
configuration.GetSection("RoutineProfiles").Bind(routineProfiles);

EnsureDefaultProfile(routineProfiles, "default", ["download", "extract"]);
EnsureDefaultProfile(routineProfiles, "move-extract", ["download", "extract", "move"]);
EnsureDefaultProfile(routineProfiles, "download-only", ["download"]);
EnsureDefaultProfile(routineProfiles, "full", ["download", "extract", "move", "cleanup"]);
EnsureDefaultProfile(routineProfiles, "restore", ["download", "extract", "restore-sql"]);

var services = new ServiceCollection();
services.RegisterHandlersFromAssemblyContaining(typeof(Program));
services.AddSingleton(routineProfiles);
services.AddScoped<IEventPublisher, EventPublisher>();
services.AddScoped<DownloadService>();
services.AddScoped<ExtractionService>();
services.AddScoped<CompressionService>();
services.AddScoped<FileMoveService>();
services.AddScoped<SqlServerRestoreService>();

services.AddScoped<IRoutineStep, DownloadRoutineStep>();
services.AddScoped<IRoutineStep, ExtractRoutineStep>();
services.AddScoped<IRoutineStep, MoveRoutineStep>();
services.AddScoped<IRoutineStep, CleanupRoutineStep>();
services.AddScoped<IRoutineStep, RestoreSqlRoutineStep>();
services.AddScoped<RoutineStepCatalog>();

services.AddLogging(config =>
{
    config.ClearProviders();
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
    config.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
});

services.AddHttpClient("", http =>
{
    http.Timeout = TimeSpan.FromSeconds(10);
});

var registrar = new TypeRegistrar(services);

var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.SetApplicationName("file-cli");

    config.AddCommand<DownloadCommand>("download")
        .WithDescription("Download de arquivo")
        .WithExample(new[] { "download", "--url", "http://site/file.zip", "--output", "c:\\temp\\file.zip" });

    config.AddCommand<ExtractCommand>("extract")
        .WithDescription("Extrai um arquivo zip");

    config.AddCommand<MoveCommand>("move")
        .WithDescription("Move arquivo ou pasta para outro diretório");

    config.AddCommand<RestoreSqlCommand>("restore-sql")
        .WithDescription("Restaura arquivo .bak no SQL Server")
        .WithExample(new[] { "restore-sql", "--backup", "c:\\temp\\db.bak", "--sql-server", ".\\SQLEXPRESS", "--sql-db", "MinhaBase", "--sql-trusted", "--sql-replace" });

    config.AddCommand<RoutineCommand>("routine")
        .WithDescription("Executa rotina por steps customizáveis (OCP)")
        .WithExample(new[] { "routine", "--url", "http://site/file.zip", "--move-extract", "--move-to", "c:\\demandas\\entrada" })
        .WithExample(new[] { "routine", "--url", "http://site/file.zip", "--steps", "download,extract,restore-sql", "--sql-server", ".\\SQLEXPRESS", "--sql-db", "MinhaBase", "--sql-trusted", "--sql-replace" });

    config.AddCommand<ProfilesCommand>("profiles")
        .WithDescription("Lista profiles e steps disponíveis para personalização");
});

await app.RunAsync(args);

static void EnsureDefaultProfile(RoutineProfilesOptions options, string profileName, string[] steps)
{
    if (!options.Profiles.ContainsKey(profileName))
    {
        options.Profiles[profileName] = steps;
    }
}
