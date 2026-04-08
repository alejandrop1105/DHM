using Dapper;
using DHM.Domain.Entities;
using DHM.Domain.Interfaces;
using DHM.Infrastructure.Data;

namespace DHM.Infrastructure.Repositories;

public class TagRepository : ITagRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public TagRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Tag>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Tag>(@"
            SELECT t.*, g.Name as GroupName 
            FROM Tags t
            LEFT JOIN TagGroups g ON t.GroupId = g.Id
            ORDER BY g.SortOrder, g.Name, t.Name");
    }

    public async Task<Tag?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Tag>(@"
            SELECT t.*, g.Name as GroupName 
            FROM Tags t
            LEFT JOIN TagGroups g ON t.GroupId = g.Id
            WHERE t.Id = @Id", new { Id = id.ToString() });
    }

    public async Task<Guid> CreateAsync(Tag tag)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            INSERT INTO Tags (Id, Name, Color, GroupId, CreatedAt)
            VALUES (@Id, @Name, @Color, @GroupId, @CreatedAt)",
            new
            {
                Id = tag.Id.ToString(),
                tag.Name,
                tag.Color,
                GroupId = tag.GroupId.ToString(),
                CreatedAt = tag.CreatedAt.ToString("o")
            });
        return tag.Id;
    }

    public async Task UpdateAsync(Tag tag)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            UPDATE Tags SET Name = @Name, Color = @Color, GroupId = @GroupId
            WHERE Id = @Id",
            new { tag.Name, tag.Color, GroupId = tag.GroupId.ToString(), Id = tag.Id.ToString() });
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM Tags WHERE Id = @Id", new { Id = id.ToString() });
    }

    public async Task<IEnumerable<Tag>> GetByTenantIdAsync(Guid tenantId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Tag>(@"
            SELECT t.* FROM Tags t
            INNER JOIN TenantTags tt ON t.Id = tt.TagId
            WHERE tt.TenantId = @TenantId",
            new { TenantId = tenantId.ToString() });
    }

    public async Task AssignTagsToTenantAsync(Guid tenantId, IEnumerable<Guid> tagIds)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Eliminar asignaciones actuales
            await connection.ExecuteAsync(
                "DELETE FROM TenantTags WHERE TenantId = @TenantId",
                new { TenantId = tenantId.ToString() },
                transaction);

            // Insertar nuevas asignaciones
            foreach (var tagId in tagIds)
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO TenantTags (TenantId, TagId)
                    VALUES (@TenantId, @TagId)",
                    new { TenantId = tenantId.ToString(), TagId = tagId.ToString() },
                    transaction);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
