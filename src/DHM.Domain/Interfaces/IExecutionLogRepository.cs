using DHM.Domain.Entities;

namespace DHM.Domain.Interfaces;

public interface IExecutionLogRepository
{
    Task<IEnumerable<ExecutionLog>> GetByTestIdAsync(Guid testId, int limit = 100);
    Task<IEnumerable<ExecutionLog>> GetByTenantIdAsync(Guid tenantId, int limit = 100);
    Task<IEnumerable<ExecutionLog>> GetRecentAsync(Guid tenantId, DateTime since);
    Task<Guid> CreateAsync(ExecutionLog log);
    Task<int> GetPassedCountAsync(Guid tenantId, DateTime since);
    Task<int> GetTotalCountAsync(Guid tenantId, DateTime since);
    Task<IEnumerable<(DateTime Date, int FailCount)>> GetFailureTrendAsync(Guid tenantId, DateTime since, DateTime until);
}
