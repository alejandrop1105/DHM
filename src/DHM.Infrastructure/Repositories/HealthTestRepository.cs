using Dapper;
using DHM.Domain.Entities;
using DHM.Domain.Interfaces;
using DHM.Infrastructure.Data;

namespace DHM.Infrastructure.Repositories;

public class HealthTestRepository : IHealthTestRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public HealthTestRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<HealthTest>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<HealthTest>("SELECT * FROM HealthTests ORDER BY Name");
    }

    public async Task<IEnumerable<HealthTest>> GetByTenantIdAsync(Guid tenantId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<HealthTest>(
            "SELECT * FROM HealthTests WHERE TenantId = @TenantId ORDER BY Name",
            new { TenantId = tenantId.ToString() });
    }

    public async Task<HealthTest?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<HealthTest>(
            "SELECT * FROM HealthTests WHERE Id = @Id", new { Id = id.ToString() });
    }

    public async Task<IEnumerable<HealthTest>> GetActiveTestsDueForExecutionAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        // Obtener tests activos cuya última ejecución fue hace más tiempo que su frecuencia
        return await connection.QueryAsync<HealthTest>(@"
            SELECT ht.* FROM HealthTests ht
            INNER JOIN Tenants t ON ht.TenantId = t.Id AND t.IsActive = 1
            WHERE ht.IsActive = 1
            AND (
                NOT EXISTS (
                    SELECT 1 FROM ExecutionLogs el
                    WHERE el.TestId = ht.Id
                )
                OR (
                    SELECT MAX(el.ExecutedAt) FROM ExecutionLogs el
                    WHERE el.TestId = ht.Id
                ) <= datetime('now', '-' || ht.FrequencySeconds || ' seconds')
            )");
    }

    public async Task<Guid> CreateAsync(HealthTest test)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            INSERT INTO HealthTests (Id, TenantId, Name, Description, SqlQuery, ExpectedResultType, ExpectedValue, RemediationSql, FrequencySeconds, IsActive, CreatedAt, UpdatedAt)
            VALUES (@Id, @TenantId, @Name, @Description, @SqlQuery, @ExpectedResultType, @ExpectedValue, @RemediationSql, @FrequencySeconds, @IsActive, @CreatedAt, @UpdatedAt)",
            new
            {
                Id = test.Id.ToString(),
                TenantId = test.TenantId.ToString(),
                test.Name,
                test.Description,
                test.SqlQuery,
                ExpectedResultType = (int)test.ExpectedResultType,
                test.ExpectedValue,
                test.RemediationSql,
                test.FrequencySeconds,
                IsActive = test.IsActive ? 1 : 0,
                CreatedAt = test.CreatedAt.ToString("o"),
                UpdatedAt = test.UpdatedAt?.ToString("o")
            });
        return test.Id;
    }

    public async Task UpdateAsync(HealthTest test)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            UPDATE HealthTests SET TenantId = @TenantId, Name = @Name, Description = @Description,
            SqlQuery = @SqlQuery, ExpectedResultType = @ExpectedResultType, ExpectedValue = @ExpectedValue,
            RemediationSql = @RemediationSql, FrequencySeconds = @FrequencySeconds,
            IsActive = @IsActive, UpdatedAt = @UpdatedAt
            WHERE Id = @Id",
            new
            {
                Id = test.Id.ToString(),
                TenantId = test.TenantId.ToString(),
                test.Name,
                test.Description,
                test.SqlQuery,
                ExpectedResultType = (int)test.ExpectedResultType,
                test.ExpectedValue,
                test.RemediationSql,
                test.FrequencySeconds,
                IsActive = test.IsActive ? 1 : 0,
                UpdatedAt = DateTime.UtcNow.ToString("o")
            });
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM HealthTests WHERE Id = @Id", new { Id = id.ToString() });
    }
}
