using Dapper;
using DHM.Domain.Entities;
using DHM.Domain.Interfaces;
using DHM.Infrastructure.Data;

namespace DHM.Infrastructure.Repositories;

public class TagGroupRepository : ITagGroupRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public TagGroupRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<TagGroup>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<TagGroup>(
            "SELECT * FROM TagGroups ORDER BY SortOrder, Name");
    }

    public async Task<TagGroup?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<TagGroup>(
            "SELECT * FROM TagGroups WHERE Id = @Id", new { Id = id.ToString() });
    }

    public async Task<Guid> CreateAsync(TagGroup group)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            INSERT INTO TagGroups (Id, Name, Color, SortOrder, CreatedAt)
            VALUES (@Id, @Name, @Color, @SortOrder, @CreatedAt)",
            new
            {
                Id = group.Id.ToString(),
                group.Name,
                group.Color,
                group.SortOrder,
                CreatedAt = group.CreatedAt.ToString("o")
            });
        return group.Id;
    }

    public async Task UpdateAsync(TagGroup group)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            UPDATE TagGroups SET Name = @Name, Color = @Color, SortOrder = @SortOrder
            WHERE Id = @Id",
            new { group.Name, group.Color, group.SortOrder, Id = group.Id.ToString() });
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "DELETE FROM TagGroups WHERE Id = @Id", new { Id = id.ToString() });
    }
}
