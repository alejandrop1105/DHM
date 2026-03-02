using Dapper;
using DHM.Domain.Entities;
using DHM.Domain.Interfaces;
using DHM.Infrastructure.Data;

namespace DHM.Infrastructure.Repositories;

public class ExecutionLogRepository : IExecutionLogRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public ExecutionLogRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ExecutionLog>> GetByTestIdAsync(Guid testId, int limit = 100)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<ExecutionLog>(
            "SELECT * FROM ExecutionLogs WHERE TestId = @TestId ORDER BY ExecutedAt DESC LIMIT @Limit",
            new { TestId = testId.ToString(), Limit = limit });
    }

    public async Task<IEnumerable<ExecutionLog>> GetByTenantIdAsync(Guid tenantId, int limit = 100)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<ExecutionLog>(
            "SELECT * FROM ExecutionLogs WHERE TenantId = @TenantId ORDER BY ExecutedAt DESC LIMIT @Limit",
            new { TenantId = tenantId.ToString(), Limit = limit });
    }

    public async Task<IEnumerable<ExecutionLog>> GetRecentAsync(Guid tenantId, DateTime since)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<ExecutionLog>(
            "SELECT * FROM ExecutionLogs WHERE TenantId = @TenantId AND ExecutedAt >= @Since ORDER BY ExecutedAt DESC",
            new { TenantId = tenantId.ToString(), Since = since.ToString("o") });
    }

    public async Task<Guid> CreateAsync(ExecutionLog log)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            INSERT INTO ExecutionLogs (Id, TestId, TenantId, ExecutedAt, Passed, ResultValue, ErrorMessage, RemediationApplied, DurationMs)
            VALUES (@Id, @TestId, @TenantId, @ExecutedAt, @Passed, @ResultValue, @ErrorMessage, @RemediationApplied, @DurationMs)",
            new
            {
                Id = log.Id.ToString(),
                TestId = log.TestId.ToString(),
                TenantId = log.TenantId.ToString(),
                ExecutedAt = log.ExecutedAt.ToString("o"),
                Passed = log.Passed ? 1 : 0,
                log.ResultValue,
                log.ErrorMessage,
                RemediationApplied = log.RemediationApplied ? 1 : 0,
                log.DurationMs
            });
        return log.Id;
    }

    public async Task<int> GetPassedCountAsync(Guid tenantId, DateTime since)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<int>(
            "SELECT COUNT(*) FROM ExecutionLogs WHERE TenantId = @TenantId AND ExecutedAt >= @Since AND Passed = 1",
            new { TenantId = tenantId.ToString(), Since = since.ToString("o") });
    }

    public async Task<int> GetTotalCountAsync(Guid tenantId, DateTime since)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<int>(
            "SELECT COUNT(*) FROM ExecutionLogs WHERE TenantId = @TenantId AND ExecutedAt >= @Since",
            new { TenantId = tenantId.ToString(), Since = since.ToString("o") });
    }

    public async Task<IEnumerable<(DateTime Date, int FailCount)>> GetFailureTrendAsync(Guid tenantId, DateTime since, DateTime until)
    {
        using var connection = _connectionFactory.CreateConnection();
        var results = await connection.QueryAsync<dynamic>(@"
            SELECT DATE(ExecutedAt) as DateStr, COUNT(*) as FailCount
            FROM ExecutionLogs
            WHERE TenantId = @TenantId AND ExecutedAt >= @Since AND ExecutedAt <= @Until AND Passed = 0
            GROUP BY DATE(ExecutedAt)
            ORDER BY DateStr",
            new { TenantId = tenantId.ToString(), Since = since.ToString("o"), Until = until.ToString("o") });

        return results.Select(r => (
            Date: DateTime.Parse((string)r.DateStr),
            FailCount: (int)(long)r.FailCount
        ));
    }
}
