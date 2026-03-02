using DHM.Domain.Enums;

namespace DHM.Domain.Entities;

/// <summary>
/// Representa un test de salud que se ejecuta contra la BD de un tenant.
/// </summary>
public class HealthTest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>Sentencia SQL de consulta para evaluar la salud.</summary>
    public string SqlQuery { get; set; } = string.Empty;

    /// <summary>Tipo de resultado esperado (valor único, lista, existencia, conteo).</summary>
    public ExpectedResultType ExpectedResultType { get; set; } = ExpectedResultType.Exists;

    /// <summary>Valor esperado (para UniqueValue o Count). Null si solo se evalúa existencia.</summary>
    public string? ExpectedValue { get; set; }

    /// <summary>Sentencia SQL de remediación que se ejecuta si el test falla (self-healing).</summary>
    public string? RemediationSql { get; set; }

    /// <summary>Frecuencia de ejecución en segundos.</summary>
    public int FrequencySeconds { get; set; } = 60;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navegación (no persistida, solo para uso en memoria)
    public Tenant? Tenant { get; set; }
}
