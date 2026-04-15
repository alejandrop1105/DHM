using Dapper;
using DHM.Domain.Entities;
using DHM.Domain.Interfaces;
using DHM.Infrastructure.Data;

namespace DHM.Infrastructure.Repositories;

public class QueryGroupRepository : IQueryGroupRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public QueryGroupRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<QueryGroup>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<QueryGroup>("SELECT * FROM QueryGroups ORDER BY Name");
    }

    public async Task<QueryGroup?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<QueryGroup>(
            "SELECT * FROM QueryGroups WHERE Id = @Id", new { Id = id.ToString() });
    }

    public async Task<Guid> CreateAsync(QueryGroup group)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            INSERT INTO QueryGroups (Id, Name, Description, CreatedAt)
            VALUES (@Id, @Name, @Description, @CreatedAt)",
            new
            {
                Id = group.Id.ToString(),
                group.Name,
                group.Description,
                CreatedAt = group.CreatedAt.ToString("o")
            });
        return group.Id;
    }

    public async Task UpdateAsync(QueryGroup group)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            UPDATE QueryGroups 
            SET Name = @Name, Description = @Description
            WHERE Id = @Id",
            new
            {
                group.Name,
                group.Description,
                Id = group.Id.ToString()
            });
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM QueryGroups WHERE Id = @Id", new { Id = id.ToString() });
    }
}
