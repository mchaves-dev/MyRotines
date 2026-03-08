using MyRotines.Application.Services;

namespace MyRotines.Application.Routines.Steps;

public sealed class ExtractRoutineStep(ExtractionService extractionService) : IRoutineStep
{
    public string Key => "extract";
    public string Description => "Extrai o arquivo zip para uma pasta local";

    public async Task ExecuteAsync(RoutineExecutionContext context, CancellationToken cancellationToken)
    {
        if (!File.Exists(context.OutputPath))
        {
            throw new FileNotFoundException($"Arquivo não encontrado para extração: {context.OutputPath}");
        }

        var outputDirectory = Path.GetDirectoryName(context.OutputPath) ?? context.WorkDirectory;
        var destination = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(context.OutputPath));

        try
        {
            await extractionService.ExtractAsync(context.OutputPath, destination, cancellationToken);
        }
        catch (InvalidDataException ex)
        {
            throw new InvalidDataException(
                "Falha ao extrair ZIP. Verifique se a URL realmente baixa um .zip e se no CMD ela está entre aspas (quando contém '&'). " +
                ex.Message,
                ex);
        }

        context.ExtractedDirectoryPath = destination;
        context.FinalPath = destination;
    }
}
