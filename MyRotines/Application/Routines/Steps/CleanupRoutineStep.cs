namespace MyRotines.Application.Routines.Steps;

public sealed class CleanupRoutineStep : IRoutineStep
{
    public string Key => "cleanup";
    public string Description => "Remove o arquivo baixado (.zip)";

    public Task ExecuteAsync(RoutineExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!context.CleanupDownloadedFile)
        {
            return Task.CompletedTask;
        }

        if (File.Exists(context.OutputPath))
        {
            File.Delete(context.OutputPath);
        }

        return Task.CompletedTask;
    }
}
