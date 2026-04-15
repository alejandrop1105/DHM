using Dapper;
using DHM.Domain.Entities;
using DHM.Domain.Interfaces;
using DHM.Infrastructure.Data;

namespace DHM.Infrastructure.Repositories;

public class SavedQueryRepository : ISavedQueryRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SavedQueryRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<SavedQuery>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<SavedQuery>(@"
            SELECT * FROM SavedQueries ORDER BY Name");
    }

    public async Task<SavedQuery?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<SavedQuery>(@"
            SELECT * FROM SavedQueries WHERE Id = @Id", new { Id = id.ToString() });
    }

    public async Task<Guid> CreateAsync(SavedQuery savedQuery)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            INSERT INTO SavedQueries (Id, Name, Description, SqlText, TenantIds, GroupName, CreatedAt, UpdatedAt)
            VALUES (@Id, @Name, @Description, @SqlText, @TenantIds, @GroupName, @CreatedAt, @UpdatedAt)",
            new
            {
                Id = savedQuery.Id.ToString(),
                savedQuery.Name,
                savedQuery.Description,
                savedQuery.SqlText,
                savedQuery.TenantIds,
                savedQuery.GroupName,
                CreatedAt = savedQuery.CreatedAt.ToString("o"),
                UpdatedAt = savedQuery.UpdatedAt?.ToString("o")
            });
        return savedQuery.Id;
    }

    public async Task UpdateAsync(SavedQuery savedQuery)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            UPDATE SavedQueries 
            SET Name = @Name, Description = @Description, SqlText = @SqlText, TenantIds = @TenantIds, GroupName = @GroupName, UpdatedAt = @UpdatedAt
            WHERE Id = @Id",
            new
            {
                savedQuery.Name,
                savedQuery.Description,
                savedQuery.SqlText,
                savedQuery.TenantIds,
                savedQuery.GroupName,
                UpdatedAt = savedQuery.UpdatedAt?.ToString("o"),
                Id = savedQuery.Id.ToString()
            });
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM SavedQueries WHERE Id = @Id", new { Id = id.ToString() });
    }

    public async Task RenameGroupAsync(string oldName, string newName)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            UPDATE SavedQueries 
            SET GroupName = @NewName 
            WHERE GroupName = @OldName", 
            new { OldName = oldName, NewName = newName });
    }

    public async Task ClearGroupAsync(string groupName)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            UPDATE SavedQueries 
            SET GroupName = NULL 
            WHERE GroupName = @GroupName", 
            new { GroupName = groupName });
    }
}
