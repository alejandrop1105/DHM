using DHM.Application.DTOs;

namespace DHM.Application.Interfaces;

public interface ISqlExecutionService
{
    /// <summary>
    /// Ejecuta una consulta SQL en una lista de Tenants especificados y retorna el resultado por cada uno.
    /// </summary>
    Task<IEnumerable<TenantExecutionResultDto>> ExecuteAcrossTenantsAsync(IEnumerable<Guid> tenantIds, string sql);
}
