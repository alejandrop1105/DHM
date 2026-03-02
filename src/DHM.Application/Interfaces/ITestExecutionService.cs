using DHM.Application.DTOs;

namespace DHM.Application.Interfaces;

public interface ITestExecutionService
{
    /// <summary>Ejecuta un test específico y retorna el log de ejecución.</summary>
    Task<ExecutionLogDto> ExecuteTestAsync(Guid testId);

    /// <summary>Ejecuta todos los tests pendientes para un tenant.</summary>
    Task<IEnumerable<ExecutionLogDto>> ExecuteAllPendingTestsAsync();
}
