using DHM.Domain.Enums;

namespace DHM.Domain.Entities;

/// <summary>
/// Representa un tenant (cliente) cuya base de datos externa se monitorea.
/// </summary>
public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public DatabaseProvider DatabaseProvider { get; set; } = DatabaseProvider.SqlServer;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
