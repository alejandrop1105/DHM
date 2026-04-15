using DHM.Application.DTOs;
using DHM.Application.Interfaces;
using DHM.Domain.Entities;
using DHM.Domain.Interfaces;

namespace DHM.Application.Services;

public class QueryGroupService : IQueryGroupService
{
    private readonly IQueryGroupRepository _repository;
    private readonly ISavedQueryRepository _queryRepository;

    public QueryGroupService(IQueryGroupRepository repository, ISavedQueryRepository queryRepository)
    {
        _repository = repository;
        _queryRepository = queryRepository;
    }

    public async Task<IEnumerable<QueryGroupDto>> GetAllAsync()
    {
        var groups = await _repository.GetAllAsync();
        var queries = await _queryRepository.GetAllAsync();
        
        return groups.Select(g => new QueryGroupDto
        {
            Id = g.Id,
            Name = g.Name,
            Description = g.Description,
            CreatedAt = g.CreatedAt,
            QueryCount = queries.Count(q => q.GroupName == g.Name)
        });
    }

    public async Task<Guid> CreateAsync(QueryGroupDto dto)
    {
        var group = new QueryGroup
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };
        return await _repository.CreateAsync(group);
    }

    public async Task UpdateAsync(QueryGroupDto dto)
    {
        var existing = await _repository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Grupo {dto.Id} no encontrado.");

        var oldName = existing.Name;
        existing.Name = dto.Name;
        existing.Description = dto.Description;

        await _repository.UpdateAsync(existing);

        // Si cambió el nombre, actualizar las consultas asociadas
        if (oldName != dto.Name)
        {
            await _queryRepository.RenameGroupAsync(oldName, dto.Name);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing != null)
        {
            await _queryRepository.ClearGroupAsync(existing.Name);
            await _repository.DeleteAsync(id);
        }
    }

    public async Task RenameGroupInQueriesAsync(string oldName, string newName)
    {
        await _queryRepository.RenameGroupAsync(oldName, newName);
    }

    public async Task ClearGroupInQueriesAsync(string groupName)
    {
        await _queryRepository.ClearGroupAsync(groupName);
    }
}
