using System.Data;
using DHM.Domain.Enums;

namespace DHM.Domain.Interfaces;

/// <summary>
/// Servicio para conectarse y ejecutar queries contra bases de datos externas de tenants.
/// Soporta cualquier BD relacional que permita ADO.NET.
/// </summary>
public interface IExternalDatabaseService
{
    /// <summary>Crea una conexión a la BD externa del tenant.</summary>
    IDbConnection CreateConnection(string connectionString, DatabaseProvider provider);

    /// <summary>Ejecuta una query SELECT y retorna los resultados.</summary>
    Task<IEnumerable<dynamic>> ExecuteQueryAsync(string connectionString, DatabaseProvider provider, string sql);

    /// <summary>Ejecuta una sentencia SQL (INSERT, UPDATE, DELETE) y retorna filas afectadas.</summary>
    Task<int> ExecuteCommandAsync(string connectionString, DatabaseProvider provider, string sql);

    /// <summary>Prueba la conexión a la BD externa.</summary>
    Task<bool> TestConnectionAsync(string connectionString, DatabaseProvider provider);

    /// <summary>Retorna la lista de nombres de bases de datos disponibles en el servidor.</summary>
    Task<IEnumerable<string>> GetDatabaseNamesAsync(string connectionString, DatabaseProvider provider);
}
