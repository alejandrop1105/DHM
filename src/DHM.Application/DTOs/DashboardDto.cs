namespace DHM.Application.DTOs;

public class DashboardDto
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public double HealthScore { get; set; }
    public int TotalTests { get; set; }
    public int PassedTests { get; set; }
    public int FailedTests { get; set; }
    public List<TestStatusCard> TestCards { get; set; } = new();
    public List<TrendDataPoint> TrendData { get; set; } = new();
}

public class TestStatusCard
{
    public Guid TestId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public bool IsPassing { get; set; }
    public DateTime? LastExecutedAt { get; set; }
    public string? LastResultValue { get; set; }
    public string? LastErrorMessage { get; set; }
    public long LastDurationMs { get; set; }
}

public class TrendDataPoint
{
    public DateTime Date { get; set; }
    public int FailCount { get; set; }
    public int PassCount { get; set; }
    public double HealthScore { get; set; }
}
