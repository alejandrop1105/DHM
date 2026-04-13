$ErrorActionPreference = 'Stop'
Add-Type -Path "src\DHM.Web\bin\Debug\net10.0\Microsoft.Data.Sqlite.dll"
$connString = "Data Source=src\DHM.Web\bin\Debug\net10.0\Data\dhm_system.db"
$conn = [Microsoft.Data.Sqlite.SqliteConnection]::new($connString)
$conn.Open()

$cmd = $conn.CreateCommand()

# Borrar tags asociados al grupo Produccion
$cmd.CommandText = "DELETE FROM TenantTags WHERE TagId IN (SELECT Id FROM Tags WHERE GroupId IN (SELECT Id FROM TagGroups WHERE Name = 'Produccion'));"
$cmd.ExecuteNonQuery() | Out-Null

$cmd.CommandText = "DELETE FROM Tags WHERE GroupId IN (SELECT Id FROM TagGroups WHERE Name = 'Produccion');"
$cmd.ExecuteNonQuery() | Out-Null

# Borrar el grupo Produccion
$cmd.CommandText = "DELETE FROM TagGroups WHERE Name = 'Produccion';"
$cmd.ExecuteNonQuery() | Out-Null

# Borrar tags huérfanos llamados 'Produccion' o 'Public 1', etc., si el usuario los creó sueltos
$cmd.CommandText = "DELETE FROM TenantTags WHERE TagId IN (SELECT Id FROM Tags WHERE Name LIKE 'Produccion%');"
$cmd.ExecuteNonQuery() | Out-Null

$cmd.CommandText = "DELETE FROM Tags WHERE Name LIKE 'Produccion%';"
$cmd.ExecuteNonQuery() | Out-Null

$conn.Close()
Write-Host "Etiquetas y grupo eliminados correctamente."
