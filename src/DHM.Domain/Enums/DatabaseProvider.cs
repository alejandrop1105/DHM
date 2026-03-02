namespace DHM.Domain.Enums;

/// <summary>
/// Proveedores de base de datos soportados para monitoreo.
/// Se usa "Generic" para cualquier BD relacional que soporte ADO.NET.
/// </summary>
public enum DatabaseProvider
{
    SqlServer,
    PostgreSql,
    MySql,
    Sqlite,
    Generic
}
