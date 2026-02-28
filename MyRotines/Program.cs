using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyRotines.Application;
using MyRotines.Application.Commands;
using MyRotines.Application.Extensions;
using MyRotines.Application.Services;
using MyRotines.Domain.Events;
using Spectre.Console.Cli;

var services = new ServiceCollection();
services.RegisterHandlersFromAssemblyContaining(typeof(Program));
services.AddScoped<IEventPublisher, EventPublisher>();
services.AddScoped<DownloadService>();
services.AddScoped<ExtractionService>();
services.AddScoped<CompressionService>();

// Logging
services.AddLogging(config =>
{
    config.ClearProviders();
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});

services.AddHttpClient("", http =>
{
    http.Timeout = TimeSpan.FromSeconds(10);
});

var provider = services.BuildServiceProvider();

var registrar = new TypeRegistrar(services);

var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.SetApplicationName("file-cli");

    config.AddCommand<DownloadCommand>("download")
        .WithDescription("Download de arquivo")
        .WithExample(new [] { "download", "--url", "http://site/file.zip", "--output", "c:\\temp\\file.zip" });

    config.AddCommand<ExtractCommand>("extract")
        .WithDescription("Extrai um arquivo zip");
});

await app.RunAsync(args);