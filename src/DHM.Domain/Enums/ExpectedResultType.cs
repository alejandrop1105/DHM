namespace DHM.Domain.Enums;

/// <summary>
/// Tipo de resultado esperado para un test de salud.
/// </summary>
public enum ExpectedResultType
{
    /// <summary>Se espera un valor único exacto.</summary>
    UniqueValue,

    /// <summary>Se espera una lista de resultados (al menos 1 fila).</summary>
    List,

    /// <summary>Se esperan resultados (existencia de filas).</summary>
    Exists,

    /// <summary>Se espera un conteo numérico específico.</summary>
    Count
}
