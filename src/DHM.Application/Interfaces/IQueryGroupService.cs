namespace DHM.Application.Interfaces;

using DHM.Application.DTOs;

public interface IQueryGroupService
{
    Task<IEnumerable<QueryGroupDto>> GetAllAsync();
    Task<Guid> CreateAsync(QueryGroupDto dto);
    Task UpdateAsync(QueryGroupDto dto);
    Task DeleteAsync(Guid id);
    Task RenameGroupInQueriesAsync(string oldName, string newName);
    Task ClearGroupInQueriesAsync(string groupName);
}
