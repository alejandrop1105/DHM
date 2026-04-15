using DHM.Domain.Entities;

namespace DHM.Domain.Interfaces;

public interface ISavedQueryRepository
{
    Task<IEnumerable<SavedQuery>> GetAllAsync();
    Task<SavedQuery?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(SavedQuery savedQuery);
    Task UpdateAsync(SavedQuery savedQuery);
    Task DeleteAsync(Guid id);
    Task RenameGroupAsync(string oldName, string newName);
    Task ClearGroupAsync(string groupName);
}
