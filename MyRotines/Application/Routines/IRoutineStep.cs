namespace MyRotines.Application.Routines;

public interface IRoutineStep
{
    string Key { get; }
    string Description { get; }
    Task ExecuteAsync(RoutineExecutionContext context, CancellationToken cancellationToken);
}
