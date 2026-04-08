using DHM.Application.DTOs;
using DHM.Application.Interfaces;
using DHM.Domain.Entities;
using DHM.Domain.Interfaces;

namespace DHM.Application.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<IEnumerable<TagDto>> GetAllAsync()
    {
        var tags = await _tagRepository.GetAllAsync();
        return tags.Select(MapToDto);
    }

    public async Task<TagDto?> GetByIdAsync(Guid id)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        return tag == null ? null : MapToDto(tag);
    }

    public async Task<Guid> CreateAsync(TagDto dto)
    {
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Color = dto.Color,
            GroupId = dto.GroupId,
            CreatedAt = DateTime.UtcNow
        };
        return await _tagRepository.CreateAsync(tag);
    }

    public async Task UpdateAsync(TagDto dto)
    {
        var tag = await _tagRepository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Etiqueta {dto.Id} no encontrada.");

        tag.Name = dto.Name;
        tag.Color = dto.Color;
        tag.GroupId = dto.GroupId;

        await _tagRepository.UpdateAsync(tag);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _tagRepository.DeleteAsync(id);
    }

    private static TagDto MapToDto(Tag tag) => new()
    {
        Id = tag.Id,
        Name = tag.Name,
        Color = tag.Color,
        GroupId = tag.GroupId,
        GroupName = tag.GroupName
    };
}
