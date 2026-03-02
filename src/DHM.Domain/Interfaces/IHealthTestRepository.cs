using DHM.Domain.Entities;

namespace DHM.Domain.Interfaces;

public interface IHealthTestRepository
{
    Task<IEnumerable<HealthTest>> GetAllAsync();
    Task<IEnumerable<HealthTest>> GetByTenantIdAsync(Guid tenantId);
    Task<HealthTest?> GetByIdAsync(Guid id);
    Task<IEnumerable<HealthTest>> GetActiveTestsDueForExecutionAsync();
    Task<Guid> CreateAsync(HealthTest test);
    Task UpdateAsync(HealthTest test);
    Task DeleteAsync(Guid id);
}
