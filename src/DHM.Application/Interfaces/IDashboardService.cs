using DHM.Application.DTOs;

namespace DHM.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(Guid tenantId);
    Task<IEnumerable<DashboardDto>> GetAllDashboardsAsync();
    Task<IEnumerable<TrendDataPoint>> GetTrendDataAsync(Guid tenantId, int days = 7);
}
