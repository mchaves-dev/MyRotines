namespace MyRotines.Application.Routines;

public sealed class RoutineProfilesOptions
{
    public Dictionary<string, string[]> Profiles { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
