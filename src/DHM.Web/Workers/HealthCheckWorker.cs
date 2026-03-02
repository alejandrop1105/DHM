using DHM.Application.Interfaces;
using DHM.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DHM.Web.Workers;

/// <summary>
/// Servicio de fondo que ejecuta tests de salud según su frecuencia configurada
/// y notifica los resultados en tiempo real a través de SignalR.
/// </summary>
public class HealthCheckWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<MonitorHub> _hubContext;
    private readonly ILogger<HealthCheckWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(10);

    public HealthCheckWorker(
        IServiceScopeFactory scopeFactory,
        IHubContext<MonitorHub> hubContext,
        ILogger<HealthCheckWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("HealthCheckWorker iniciado. Intervalo de verificación: {Interval}s", _checkInterval.TotalSeconds);

        // Esperar a que la app esté lista
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExecutePendingTestsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error en el ciclo de HealthCheckWorker");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("HealthCheckWorker detenido.");
    }

    private async Task ExecutePendingTestsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var executionService = scope.ServiceProvider.GetRequiredService<ITestExecutionService>();
        var dashboardService = scope.ServiceProvider.GetRequiredService<IDashboardService>();

        var results = await executionService.ExecuteAllPendingTestsAsync();
        var resultList = results.ToList();

        if (resultList.Count == 0)
        {
            return; // Sin tests pendientes, nada que notificar
        }

        _logger.LogInformation("Se ejecutaron {Count} tests", resultList.Count);

        // Agrupar resultados por tenant para notificar por grupo de SignalR
        var byTenant = resultList.GroupBy(r => r.TenantId);

        foreach (var tenantGroup in byTenant)
        {
            var tenantId = tenantGroup.Key.ToString();

            foreach (var result in tenantGroup)
            {
                // Notificar al grupo del tenant
                await _hubContext.Clients.Group(tenantId).SendAsync("TestCompleted", new
                {
                    result.TestId,
                    result.TestName,
                    result.TenantId,
                    result.TenantName,
                    result.Passed,
                    result.ResultValue,
                    result.ErrorMessage,
                    result.RemediationApplied,
                    result.DurationMs,
                    ExecutedAt = result.ExecutedAt.ToString("o")
                }, ct);

                _logger.LogDebug("Notificación SignalR enviada: Test '{TestName}' - {Status}",
                    result.TestName, result.Passed ? "PASSED" : "FAILED");
            }

            // Actualizar dashboard del tenant
            try
            {
                var dashboard = await dashboardService.GetDashboardAsync(tenantGroup.Key);
                await _hubContext.Clients.Group(tenantId).SendAsync("DashboardUpdated", new
                {
                    dashboard.TenantId,
                    dashboard.TenantName,
                    dashboard.HealthScore,
                    dashboard.TotalTests,
                    dashboard.PassedTests,
                    dashboard.FailedTests,
                    TestCards = dashboard.TestCards.Select(c => new
                    {
                        c.TestId,
                        c.TestName,
                        c.IsPassing,
                        LastExecutedAt = c.LastExecutedAt?.ToString("o"),
                        c.LastResultValue,
                        c.LastErrorMessage,
                        c.LastDurationMs
                    })
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener dashboard para tenant {TenantId}", tenantId);
            }
        }

        // Notificar actualización global del dashboard
        await _hubContext.Clients.Group("dashboard").SendAsync("GlobalDashboardUpdated",
            new { UpdatedAt = DateTime.UtcNow.ToString("o"), TestsExecuted = resultList.Count }, ct);
    }
}
