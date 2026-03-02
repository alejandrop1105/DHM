namespace DHM.Domain.Enums;

/// <summary>
/// Estado de ejecución de un test de salud.
/// </summary>
public enum TestStatus
{
    Pending,
    Running,
    Passed,
    Failed,
    Error
}
