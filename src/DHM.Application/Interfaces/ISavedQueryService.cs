using DHM.Application.DTOs;

namespace DHM.Application.Interfaces;

public interface ISavedQueryService
{
    Task<IEnumerable<SavedQueryDto>> GetAllAsync();
    Task<SavedQueryDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(SavedQueryDto dto);
    Task UpdateAsync(SavedQueryDto dto);
    Task DeleteAsync(Guid id);
}
