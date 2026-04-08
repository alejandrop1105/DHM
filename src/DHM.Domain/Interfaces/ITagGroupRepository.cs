using DHM.Domain.Entities;

namespace DHM.Domain.Interfaces;

public interface ITagGroupRepository
{
    Task<IEnumerable<TagGroup>> GetAllAsync();
    Task<TagGroup?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(TagGroup group);
    Task UpdateAsync(TagGroup group);
    Task DeleteAsync(Guid id);
}
