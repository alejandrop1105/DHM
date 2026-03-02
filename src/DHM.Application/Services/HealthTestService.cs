using DHM.Application.DTOs;
using DHM.Application.Interfaces;
using DHM.Domain.Entities;
using DHM.Domain.Interfaces;

namespace DHM.Application.Services;

public class HealthTestService : IHealthTestService
{
    private readonly IHealthTestRepository _testRepository;
    private readonly ITenantRepository _tenantRepository;

    public HealthTestService(IHealthTestRepository testRepository, ITenantRepository tenantRepository)
    {
        _testRepository = testRepository;
        _tenantRepository = tenantRepository;
    }

    public async Task<IEnumerable<HealthTestDto>> GetAllAsync()
    {
        var tests = await _testRepository.GetAllAsync();
        var tenants = (await _tenantRepository.GetAllAsync()).ToDictionary(t => t.Id);

        return tests.Select(t => MapToDto(t, tenants.GetValueOrDefault(t.TenantId)?.Name ?? "Desconocido"));
    }

    public async Task<IEnumerable<HealthTestDto>> GetByTenantIdAsync(Guid tenantId)
    {
        var tests = await _testRepository.GetByTenantIdAsync(tenantId);
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);

        return tests.Select(t => MapToDto(t, tenant?.Name ?? "Desconocido"));
    }

    public async Task<HealthTestDto?> GetByIdAsync(Guid id)
    {
        var test = await _testRepository.GetByIdAsync(id);
        if (test is null) return null;

        var tenant = await _tenantRepository.GetByIdAsync(test.TenantId);
        return MapToDto(test, tenant?.Name ?? "Desconocido");
    }

    public async Task<Guid> CreateAsync(HealthTestDto dto)
    {
        var test = new HealthTest
        {
            Id = Guid.NewGuid(),
            TenantId = dto.TenantId,
            Name = dto.Name,
            Description = dto.Description,
            SqlQuery = dto.SqlQuery,
            ExpectedResultType = dto.ExpectedResultType,
            ExpectedValue = dto.ExpectedValue,
            RemediationSql = dto.RemediationSql,
            FrequencySeconds = dto.FrequencySeconds,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        return await _testRepository.CreateAsync(test);
    }

    public async Task UpdateAsync(HealthTestDto dto)
    {
        var test = await _testRepository.GetByIdAsync(dto.Id)
            ?? throw new InvalidOperationException($"Test {dto.Id} no encontrado.");

        test.Name = dto.Name;
        test.Description = dto.Description;
        test.SqlQuery = dto.SqlQuery;
        test.ExpectedResultType = dto.ExpectedResultType;
        test.ExpectedValue = dto.ExpectedValue;
        test.RemediationSql = dto.RemediationSql;
        test.FrequencySeconds = dto.FrequencySeconds;
        test.IsActive = dto.IsActive;
        test.UpdatedAt = DateTime.UtcNow;

        await _testRepository.UpdateAsync(test);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _testRepository.DeleteAsync(id);
    }

    private static HealthTestDto MapToDto(HealthTest test, string tenantName) => new()
    {
        Id = test.Id,
        TenantId = test.TenantId,
        TenantName = tenantName,
        Name = test.Name,
        Description = test.Description,
        SqlQuery = test.SqlQuery,
        ExpectedResultType = test.ExpectedResultType,
        ExpectedValue = test.ExpectedValue,
        RemediationSql = test.RemediationSql,
        FrequencySeconds = test.FrequencySeconds,
        IsActive = test.IsActive,
        CreatedAt = test.CreatedAt
    };
}
