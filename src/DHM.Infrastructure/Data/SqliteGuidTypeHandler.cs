using System.Data;
using Dapper;

namespace DHM.Infrastructure.Data;

/// <summary>
/// TypeHandler para que Dapper pueda convertir automáticamente entre Guid (C#) y TEXT (SQLite).
/// SQLite almacena los GUIDs como cadenas de texto (TEXT), y este handler se encarga
/// de la serialización/deserialización transparente.
/// </summary>
public class SqliteGuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override Guid Parse(object value)
    {
        return Guid.Parse(value.ToString()!);
    }

    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.Value = value.ToString();
    }
}
