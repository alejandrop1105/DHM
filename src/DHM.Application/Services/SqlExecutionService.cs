using System.Diagnostics;
using DHM.Application.DTOs;
using DHM.Application.Interfaces;
using DHM.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DHM.Application.Services;

public class SqlExecutionService : ISqlExecutionService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IExternalDatabaseService _externalDatabaseService;
    private readonly ILogger<SqlExecutionService> _logger;

    public SqlExecutionService(
        ITenantRepository tenantRepository,
        IExternalDatabaseService externalDatabaseService,
        ILogger<SqlExecutionService> logger)
    {
        _tenantRepository = tenantRepository;
        _externalDatabaseService = externalDatabaseService;
        _logger = logger;
    }

    public async Task<IEnumerable<TenantExecutionResultDto>> ExecuteAcrossTenantsAsync(IEnumerable<Guid> tenantIds, string sql)
    {
        var results = new List<TenantExecutionResultDto>();
        var tenants = (await _tenantRepository.GetAllAsync())
            .Where(t => tenantIds.Contains(t.Id))
            .ToList();

        foreach (var tenant in tenants)
        {
            var result = new TenantExecutionResultDto
            {
                TenantId = tenant.Id,
                TenantName = tenant.Name
            };

            var sw = Stopwatch.StartNew();
            try
            {
                // Execute query
                var dynamicResults = await _externalDatabaseService.ExecuteQueryAsync(tenant.ConnectionString, tenant.DatabaseProvider, sql);
                
                // Cast to Dictionary to easily work with columns dynamically
                result.Data = dynamicResults.Cast<IDictionary<string, object>>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error ejecutando SQL en tenant {TenantName}", tenant.Name);
                result.ErrorMessage = ex.Message;
            }
            finally
            {
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
            }

            results.Add(result);
        }

        return results;
    }
}
