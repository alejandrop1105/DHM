using Microsoft.AspNetCore.SignalR;

namespace DHM.Web.Hubs;

/// <summary>
/// Hub de SignalR para notificaciones en tiempo real del monitoreo de salud.
/// Usa grupos por TenantId para filtrar las actualizaciones.
/// </summary>
public class MonitorHub : Hub
{
    private readonly ILogger<MonitorHub> _logger;

    public MonitorHub(ILogger<MonitorHub> logger)
    {
        _logger = logger;
    }

    /// <summary>Suscribir al usuario al grupo de un tenant específico.</summary>
    public async Task JoinTenantGroup(string tenantId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, tenantId);
        _logger.LogDebug("Conexión {ConnectionId} se unió al grupo del tenant {TenantId}",
            Context.ConnectionId, tenantId);
    }

    /// <summary>Desuscribir al usuario del grupo de un tenant.</summary>
    public async Task LeaveTenantGroup(string tenantId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, tenantId);
        _logger.LogDebug("Conexión {ConnectionId} dejó el grupo del tenant {TenantId}",
            Context.ConnectionId, tenantId);
    }

    /// <summary>Suscribir al usuario a actualizaciones globales del dashboard.</summary>
    public async Task JoinDashboardGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "dashboard");
        _logger.LogDebug("Conexión {ConnectionId} se unió al grupo dashboard", Context.ConnectionId);
    }

    public async Task LeaveDashboardGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "dashboard");
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Cliente conectado: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Cliente desconectado: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
