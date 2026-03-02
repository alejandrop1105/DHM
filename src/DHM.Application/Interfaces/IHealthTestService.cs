using DHM.Application.DTOs;

namespace DHM.Application.Interfaces;

public interface IHealthTestService
{
    Task<IEnumerable<HealthTestDto>> GetAllAsync();
    Task<IEnumerable<HealthTestDto>> GetByTenantIdAsync(Guid tenantId);
    Task<HealthTestDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(HealthTestDto dto);
    Task UpdateAsync(HealthTestDto dto);
    Task DeleteAsync(Guid id);
}
