namespace MyRotines.Application.Routines;

public sealed class RoutineStepCatalog(IEnumerable<IRoutineStep> steps)
{
    private readonly Dictionary<string, IRoutineStep> _steps = steps
        .GroupBy(step => step.Key, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

    public bool TryGet(string key, out IRoutineStep? step)
    {
        if (_steps.TryGetValue(key, out var resolved))
        {
            step = resolved;
            return true;
        }

        step = null;
        return false;
    }

    public IReadOnlyCollection<IRoutineStep> GetAll() => _steps.Values.OrderBy(step => step.Key).ToArray();
}
