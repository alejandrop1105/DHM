using System.Data;
using Microsoft.Data.Sqlite;

namespace DHM.Infrastructure.Data;

/// <summary>
/// Factory para crear conexiones a la base de datos del sistema (SQLite).
/// </summary>
public class SqliteConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}
