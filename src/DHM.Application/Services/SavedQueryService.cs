using DHM.Application.DTOs;
using DHM.Application.Interfaces;
using DHM.Domain.Entities;
using DHM.Domain.Interfaces;

namespace DHM.Application.Services;

public class SavedQueryService : ISavedQueryService
{
    private readonly ISavedQueryRepository _repository;

    public SavedQueryService(ISavedQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<SavedQueryDto>> GetAllAsync()
    {
        var queries = await _repository.GetAllAsync();
        return queries.Select(MapToDto);
    }

    public async Task<SavedQueryDto?> GetByIdAsync(Guid id)
    {
        var query = await _repository.GetByIdAsync(id);
        return query == null ? null : MapToDto(query);
    }

    public async Task<Guid> CreateAsync(SavedQueryDto dto)
    {
        var query = new SavedQuery
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            SqlText = dto.SqlText,
            CreatedAt = DateTime.UtcNow
        };
        return await _repository.CreateAsync(query);
    }

    public async Task UpdateAsync(SavedQueryDto dto)
    {
        var query = await _repository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Consulta {dto.Id} no encontrada.");

        query.Name = dto.Name;
        query.Description = dto.Description;
        query.SqlText = dto.SqlText;
        query.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(query);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    private static SavedQueryDto MapToDto(SavedQuery entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        SqlText = entity.SqlText,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
