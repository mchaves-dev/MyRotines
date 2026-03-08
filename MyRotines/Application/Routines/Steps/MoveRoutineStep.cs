using MyRotines.Application.Services;

namespace MyRotines.Application.Routines.Steps;

public sealed class MoveRoutineStep(FileMoveService fileMoveService) : IRoutineStep
{
    public string Key => "move";
    public string Description => "Move arquivo/pasta para diretório final";

    public async Task ExecuteAsync(RoutineExecutionContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(context.MoveToDirectory))
        {
            throw new InvalidOperationException("Step 'move' requer o parâmetro --move-to.");
        }

        var sourcePath = context.ResolvePrimarySourcePath();
        var targetPath = await fileMoveService.MoveAsync(sourcePath, context.MoveToDirectory, overwrite: true, cancellationToken);
        context.FinalPath = targetPath;
    }
}
