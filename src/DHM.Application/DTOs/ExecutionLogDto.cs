namespace DHM.Application.DTOs;

public class ExecutionLogDto
{
    public Guid Id { get; set; }
    public Guid TestId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public bool Passed { get; set; }
    public string? ResultValue { get; set; }
    public string? ErrorMessage { get; set; }
    public bool RemediationApplied { get; set; }
    public long DurationMs { get; set; }
}
