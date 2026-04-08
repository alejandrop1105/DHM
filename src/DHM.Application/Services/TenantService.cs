using DHM.Application.DTOs;
using DHM.Application.Interfaces;
using DHM.Domain.Entities;
using DHM.Domain.Interfaces;

namespace DHM.Application.Services;

public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IExternalDatabaseService _externalDbService;
    private readonly ITagRepository _tagRepository;

    public TenantService(ITenantRepository tenantRepository, IExternalDatabaseService externalDbService, ITagRepository tagRepository)
    {
        _tenantRepository = tenantRepository;
        _externalDbService = externalDbService;
        _tagRepository = tagRepository;
    }

    public async Task<IEnumerable<TenantDto>> GetAllAsync()
    {
        var tenants = await _tenantRepository.GetAllAsync();
        var dtos = new List<TenantDto>();
        
        foreach (var tenant in tenants)
        {
            var dto = MapToDto(tenant);
            var tags = await _tagRepository.GetByTenantIdAsync(tenant.Id);
            dto.Tags = tags.Select(t => new TagDto { Id = t.Id, Name = t.Name, Color = t.Color }).ToList();
            dtos.Add(dto);
        }
        
        return dtos;
    }

    public async Task<TenantDto?> GetByIdAsync(Guid id)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);
        if (tenant is null) return null;
        
        var dto = MapToDto(tenant);
        var tags = await _tagRepository.GetByTenantIdAsync(id);
        dto.Tags = tags.Select(t => new TagDto { Id = t.Id, Name = t.Name, Color = t.Color }).ToList();
        
        return dto;
    }

    public async Task<Guid> CreateAsync(TenantDto dto)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            ConnectionString = dto.ConnectionString,
            DatabaseProvider = dto.DatabaseProvider,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        
        var id = await _tenantRepository.CreateAsync(tenant);
        
        if (dto.Tags != null && dto.Tags.Any())
        {
            await _tagRepository.AssignTagsToTenantAsync(id, dto.Tags.Select(t => t.Id));
        }
        
        return id;
    }

    public async Task UpdateAsync(TenantDto dto)
    {
        var tenant = await _tenantRepository.GetByIdAsync(dto.Id)
            ?? throw new InvalidOperationException($"Tenant {dto.Id} no encontrado.");

        tenant.Name = dto.Name;
        tenant.ConnectionString = dto.ConnectionString;
        tenant.DatabaseProvider = dto.DatabaseProvider;
        tenant.IsActive = dto.IsActive;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _tenantRepository.UpdateAsync(tenant);
        
        // Sincronizar etiquetas
        var tagIds = dto.Tags?.Select(t => t.Id) ?? Enumerable.Empty<Guid>();
        await _tagRepository.AssignTagsToTenantAsync(dto.Id, tagIds);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _tenantRepository.DeleteAsync(id);
    }

    public async Task<bool> TestConnectionAsync(Guid tenantId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId)
            ?? throw new InvalidOperationException($"Tenant {tenantId} no encontrado.");

        return await _externalDbService.TestConnectionAsync(tenant.ConnectionString, tenant.DatabaseProvider);
    }

    public async Task<IEnumerable<string>> DiscoverDatabasesAsync(string connectionString, Domain.Enums.DatabaseProvider provider)
    {
        return await _externalDbService.GetDatabaseNamesAsync(connectionString, provider);
    }

    public async Task CreateBulkAsync(IEnumerable<TenantDto> tenants)
    {
        foreach (var dto in tenants)
        {
            await CreateAsync(dto);
        }
    }

    public async Task DeleteBulkAsync(IEnumerable<Guid> ids)
    {
        foreach (var id in ids)
        {
            await DeleteAsync(id);
        }
    }

    private static TenantDto MapToDto(Tenant tenant) => new()
    {
        Id = tenant.Id,
        Name = tenant.Name,
        ConnectionString = tenant.ConnectionString,
        DatabaseProvider = tenant.DatabaseProvider,
        IsActive = tenant.IsActive,
        CreatedAt = tenant.CreatedAt,
        UpdatedAt = tenant.UpdatedAt
    };
}
