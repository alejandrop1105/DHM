using DHM.Application.DTOs;
using DHM.Application.Interfaces;
using DHM.Domain.Interfaces;

namespace DHM.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IHealthTestRepository _testRepository;
    private readonly IExecutionLogRepository _logRepository;

    public DashboardService(
        ITenantRepository tenantRepository,
        IHealthTestRepository testRepository,
        IExecutionLogRepository logRepository)
    {
        _tenantRepository = tenantRepository;
        _testRepository = testRepository;
        _logRepository = logRepository;
    }

    public async Task<DashboardDto> GetDashboardAsync(Guid tenantId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant is null) throw new InvalidOperationException($"Tenant {tenantId} no encontrado.");

        var tests = (await _testRepository.GetByTenantIdAsync(tenantId)).ToList();
        var since = DateTime.UtcNow.AddHours(-24);

        var passed = await _logRepository.GetPassedCountAsync(tenantId, since);
        var total = await _logRepository.GetTotalCountAsync(tenantId, since);
        var healthScore = total > 0 ? (double)passed / total * 100 : 100;

        // Obtener última ejecución por test
        var testCards = new List<TestStatusCard>();
        foreach (var test in tests)
        {
            var recentLogs = (await _logRepository.GetByTestIdAsync(test.Id, 1)).ToList();
            var lastLog = recentLogs.FirstOrDefault();

            testCards.Add(new TestStatusCard
            {
                TestId = test.Id,
                TestName = test.Name,
                IsPassing = lastLog?.Passed ?? true,
                LastExecutedAt = lastLog?.ExecutedAt,
                LastResultValue = lastLog?.ResultValue,
                LastErrorMessage = lastLog?.ErrorMessage,
                LastDurationMs = lastLog?.DurationMs ?? 0
            });
        }

        return new DashboardDto
        {
            TenantId = tenant.Id,
            TenantName = tenant.Name,
            HealthScore = Math.Round(healthScore, 1),
            TotalTests = tests.Count,
            PassedTests = testCards.Count(c => c.IsPassing),
            FailedTests = testCards.Count(c => !c.IsPassing),
            TestCards = testCards
        };
    }

    public async Task<IEnumerable<DashboardDto>> GetAllDashboardsAsync()
    {
        var tenants = await _tenantRepository.GetAllAsync();
        var dashboards = new List<DashboardDto>();

        foreach (var tenant in tenants.Where(t => t.IsActive))
        {
            try
            {
                var dashboard = await GetDashboardAsync(tenant.Id);
                dashboards.Add(dashboard);
            }
            catch
            {
                // Si falla obtener el dashboard de un tenant, lo saltamos
                dashboards.Add(new DashboardDto
                {
                    TenantId = tenant.Id,
                    TenantName = tenant.Name,
                    HealthScore = 0
                });
            }
        }

        return dashboards;
    }

    public async Task<IEnumerable<TrendDataPoint>> GetTrendDataAsync(Guid tenantId, int days = 7)
    {
        var since = DateTime.UtcNow.AddDays(-days);
        var until = DateTime.UtcNow;

        var failureTrend = await _logRepository.GetFailureTrendAsync(tenantId, since, until);
        var trendData = new List<TrendDataPoint>();

        foreach (var (date, failCount) in failureTrend)
        {
            var totalForDay = await _logRepository.GetTotalCountAsync(tenantId, date);
            var passCount = totalForDay - failCount;
            var score = totalForDay > 0 ? (double)passCount / totalForDay * 100 : 100;

            trendData.Add(new TrendDataPoint
            {
                Date = date,
                FailCount = failCount,
                PassCount = passCount,
                HealthScore = Math.Round(score, 1)
            });
        }

        return trendData;
    }
}
