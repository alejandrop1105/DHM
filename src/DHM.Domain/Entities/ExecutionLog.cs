namespace DHM.Domain.Entities;

/// <summary>
/// Registro de ejecución de un test de salud para auditoría y tendencias.
/// </summary>
public class ExecutionLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TestId { get; set; }
    public Guid TenantId { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public bool Passed { get; set; }
    public string? ResultValue { get; set; }
    public string? ErrorMessage { get; set; }
    public bool RemediationApplied { get; set; }
    public long DurationMs { get; set; }

    // Navegación
    public HealthTest? HealthTest { get; set; }
    public Tenant? Tenant { get; set; }
}
