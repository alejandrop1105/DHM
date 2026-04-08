using DHM.Domain.Enums;

namespace DHM.Application.DTOs;

public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public DatabaseProvider DatabaseProvider { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<TagDto> Tags { get; set; } = new();

    // Datos calculados para dashboard
    public int TotalTests { get; set; }
    public int PassingTests { get; set; }
    public int FailingTests { get; set; }
    public double HealthScore { get; set; }
}
