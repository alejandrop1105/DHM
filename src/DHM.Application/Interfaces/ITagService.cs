using DHM.Application.DTOs;

namespace DHM.Application.Interfaces;

public interface ITagService
{
    Task<IEnumerable<TagDto>> GetAllAsync();
    Task<TagDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(TagDto dto);
    Task UpdateAsync(TagDto dto);
    Task DeleteAsync(Guid id);
}
