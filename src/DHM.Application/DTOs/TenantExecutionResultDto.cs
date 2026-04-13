namespace DHM.Application.DTOs;

public class TenantExecutionResultDto
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public IEnumerable<IDictionary<string, object>>? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public long DurationMs { get; set; }
    public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);
}
