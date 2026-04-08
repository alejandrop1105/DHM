using DHM.Application.DTOs;

namespace DHM.Application.Interfaces;

public interface ITenantService
{
    Task<IEnumerable<TenantDto>> GetAllAsync();
    Task<TenantDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(TenantDto dto);
    Task UpdateAsync(TenantDto dto);
    Task DeleteAsync(Guid id);
    Task<bool> TestConnectionAsync(Guid tenantId);
    Task<IEnumerable<string>> DiscoverDatabasesAsync(string connectionString, Domain.Enums.DatabaseProvider provider);
    Task CreateBulkAsync(IEnumerable<TenantDto> tenants);
    Task DeleteBulkAsync(IEnumerable<Guid> ids);
}
