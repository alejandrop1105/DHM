using Dapper;
using DHM.Domain.Entities;
using DHM.Domain.Interfaces;
using DHM.Infrastructure.Data;

namespace DHM.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public TenantRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var tenants = await connection.QueryAsync<Tenant>("SELECT * FROM Tenants ORDER BY Name");
        return tenants;
    }

    public async Task<Tenant?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Tenant>(
            "SELECT * FROM Tenants WHERE Id = @Id", new { Id = id.ToString() });
    }

    public async Task<Guid> CreateAsync(Tenant tenant)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            INSERT INTO Tenants (Id, Name, ConnectionString, DatabaseProvider, IsActive, CreatedAt, UpdatedAt)
            VALUES (@Id, @Name, @ConnectionString, @DatabaseProvider, @IsActive, @CreatedAt, @UpdatedAt)",
            new
            {
                Id = tenant.Id.ToString(),
                tenant.Name,
                tenant.ConnectionString,
                DatabaseProvider = (int)tenant.DatabaseProvider,
                IsActive = tenant.IsActive ? 1 : 0,
                CreatedAt = tenant.CreatedAt.ToString("o"),
                UpdatedAt = tenant.UpdatedAt?.ToString("o")
            });
        return tenant.Id;
    }

    public async Task UpdateAsync(Tenant tenant)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            UPDATE Tenants SET Name = @Name, ConnectionString = @ConnectionString,
            DatabaseProvider = @DatabaseProvider, IsActive = @IsActive, UpdatedAt = @UpdatedAt
            WHERE Id = @Id",
            new
            {
                Id = tenant.Id.ToString(),
                tenant.Name,
                tenant.ConnectionString,
                DatabaseProvider = (int)tenant.DatabaseProvider,
                IsActive = tenant.IsActive ? 1 : 0,
                UpdatedAt = DateTime.UtcNow.ToString("o")
            });
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM Tenants WHERE Id = @Id", new { Id = id.ToString() });
    }
}
