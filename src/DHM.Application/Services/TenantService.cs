using DHM.Application.DTOs;
using DHM.Application.Interfaces;
using DHM.Domain.Entities;
using DHM.Domain.Interfaces;

namespace DHM.Application.Services;

public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IExternalDatabaseService _externalDbService;

    public TenantService(ITenantRepository tenantRepository, IExternalDatabaseService externalDbService)
    {
        _tenantRepository = tenantRepository;
        _externalDbService = externalDbService;
    }

    public async Task<IEnumerable<TenantDto>> GetAllAsync()
    {
        var tenants = await _tenantRepository.GetAllAsync();
        return tenants.Select(MapToDto);
    }

    public async Task<TenantDto?> GetByIdAsync(Guid id)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);
        return tenant is null ? null : MapToDto(tenant);
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
        return await _tenantRepository.CreateAsync(tenant);
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
