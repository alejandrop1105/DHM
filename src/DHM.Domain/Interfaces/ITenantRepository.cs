using DHM.Domain.Entities;

namespace DHM.Domain.Interfaces;

public interface ITenantRepository
{
    Task<IEnumerable<Tenant>> GetAllAsync();
    Task<Tenant?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(Tenant tenant);
    Task UpdateAsync(Tenant tenant);
    Task DeleteAsync(Guid id);
}
