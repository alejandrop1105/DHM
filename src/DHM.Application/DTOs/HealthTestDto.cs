using DHM.Domain.Enums;

namespace DHM.Application.DTOs;

public class HealthTestDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SqlQuery { get; set; } = string.Empty;
    public ExpectedResultType ExpectedResultType { get; set; }
    public string? ExpectedValue { get; set; }
    public string? RemediationSql { get; set; }
    public int FrequencySeconds { get; set; } = 60;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Estado actual (calculado)
    public TestStatus LastStatus { get; set; } = TestStatus.Pending;
    public DateTime? LastExecutedAt { get; set; }
    public string? LastResultValue { get; set; }
    public string? LastErrorMessage { get; set; }
}
