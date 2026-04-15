namespace DHM.Domain.Interfaces;

using DHM.Domain.Entities;

public interface IQueryGroupRepository
{
    Task<IEnumerable<QueryGroup>> GetAllAsync();
    Task<QueryGroup?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(QueryGroup group);
    Task UpdateAsync(QueryGroup group);
    Task DeleteAsync(Guid id);
}
