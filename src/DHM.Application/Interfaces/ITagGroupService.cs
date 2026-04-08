using DHM.Application.DTOs;

namespace DHM.Application.Interfaces;

public interface ITagGroupService
{
    Task<IEnumerable<TagGroupDto>> GetAllAsync();
    Task<TagGroupDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(TagGroupDto dto);
    Task UpdateAsync(TagGroupDto dto);
    Task DeleteAsync(Guid id);
}
