using MyRotines.Application.Services;

namespace MyRotines.Application.Routines.Steps;

public sealed class DownloadRoutineStep(DownloadService downloadService) : IRoutineStep
{
    public string Key => "download";
    public string Description => "Baixa o arquivo da URL informada";

    public async Task ExecuteAsync(RoutineExecutionContext context, CancellationToken cancellationToken)
    {
        await downloadService.DownloadAsync(context.Url, context.OutputPath, cancellationToken);
        context.FinalPath = context.OutputPath;
    }
}
