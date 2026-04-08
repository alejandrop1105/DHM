using DHM.Application.DTOs;
using DHM.Application.Interfaces;
using DHM.Domain.Entities;
using DHM.Domain.Interfaces;

namespace DHM.Application.Services;

public class TagGroupService : ITagGroupService
{
    private readonly ITagGroupRepository _groupRepository;

    public TagGroupService(ITagGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    public async Task<IEnumerable<TagGroupDto>> GetAllAsync()
    {
        var groups = await _groupRepository.GetAllAsync();
        return groups.Select(MapToDto);
    }

    public async Task<TagGroupDto?> GetByIdAsync(Guid id)
    {
        var group = await _groupRepository.GetByIdAsync(id);
        return group == null ? null : MapToDto(group);
    }

    public async Task<Guid> CreateAsync(TagGroupDto dto)
    {
        var group = new TagGroup
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Color = dto.Color,
            SortOrder = dto.SortOrder,
            CreatedAt = DateTime.UtcNow
        };
        return await _groupRepository.CreateAsync(group);
    }

    public async Task UpdateAsync(TagGroupDto dto)
    {
        var group = await _groupRepository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Grupo {dto.Id} no encontrado.");

        group.Name = dto.Name;
        group.Color = dto.Color;
        group.SortOrder = dto.SortOrder;

        await _groupRepository.UpdateAsync(group);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _groupRepository.DeleteAsync(id);
    }

    private static TagGroupDto MapToDto(TagGroup g) => new()
    {
        Id = g.Id,
        Name = g.Name,
        Color = g.Color,
        SortOrder = g.SortOrder
    };
}
