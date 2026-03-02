using System.Data;
using Dapper;
using DHM.Domain.Enums;
using DHM.Domain.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace DHM.Infrastructure.Services;

/// <summary>
/// Servicio para ejecutar queries contra BDs externas de tenants.
/// Soporta SQL Server, PostgreSQL, MySQL y SQLite.
/// Para cualquier otra BD relacional, se puede usar el proveedor Generic
/// siempre que se proporcione un connection string ADO.NET válido.
/// </summary>
public class ExternalDatabaseService : IExternalDatabaseService
{
    private readonly ILogger<ExternalDatabaseService> _logger;

    public ExternalDatabaseService(ILogger<ExternalDatabaseService> logger)
    {
        _logger = logger;
    }

    public IDbConnection CreateConnection(string connectionString, DatabaseProvider provider)
    {
        return provider switch
        {
            DatabaseProvider.SqlServer => new Microsoft.Data.SqlClient.SqlConnection(connectionString),
            DatabaseProvider.PostgreSql => new Npgsql.NpgsqlConnection(connectionString),
            DatabaseProvider.MySql => new MySqlConnector.MySqlConnection(connectionString),
            DatabaseProvider.Sqlite => new SqliteConnection(connectionString),
            DatabaseProvider.Generic => new SqliteConnection(connectionString), // Fallback a SQLite para genérico
            _ => throw new NotSupportedException($"Proveedor de base de datos '{provider}' no soportado.")
        };
    }

    public async Task<IEnumerable<dynamic>> ExecuteQueryAsync(string connectionString, DatabaseProvider provider, string sql)
    {
        using var connection = CreateConnection(connectionString, provider);
        connection.Open();
        _logger.LogDebug("Ejecutando query en {Provider}: {Sql}", provider, sql);
        return await connection.QueryAsync(sql);
    }

    public async Task<int> ExecuteCommandAsync(string connectionString, DatabaseProvider provider, string sql)
    {
        using var connection = CreateConnection(connectionString, provider);
        connection.Open();
        _logger.LogDebug("Ejecutando comando en {Provider}: {Sql}", provider, sql);
        return await connection.ExecuteAsync(sql);
    }

    public async Task<bool> TestConnectionAsync(string connectionString, DatabaseProvider provider)
    {
        try
        {
            using var connection = CreateConnection(connectionString, provider);
            connection.Open();
            await connection.QueryAsync("SELECT 1");
            _logger.LogInformation("Conexión exitosa a BD externa ({Provider})", provider);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al probar conexión a BD externa ({Provider})", provider);
            return false;
        }
    }
}
