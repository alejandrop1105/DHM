using DHM.Domain.Entities;

namespace DHM.Domain.Interfaces;

public interface ITagRepository
{
    Task<IEnumerable<Tag>> GetAllAsync();
    Task<Tag?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(Tag tag);
    Task UpdateAsync(Tag tag);
    Task DeleteAsync(Guid id);
    
    // Métodos para la relación con Tenants
    Task<IEnumerable<Tag>> GetByTenantIdAsync(Guid tenantId);
    Task AssignTagsToTenantAsync(Guid tenantId, IEnumerable<Guid> tagIds);
}
