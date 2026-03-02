using Dapper;
using Microsoft.Extensions.Logging;

namespace DHM.Infrastructure.Data;

/// <summary>
/// Inicializa las tablas del sistema en SQLite al arrancar la aplicación.
/// También crea una BD mock de prueba como tenant demo.
/// </summary>
public class DatabaseInitializer
{
    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(SqliteConnectionFactory connectionFactory, ILogger<DatabaseInitializer> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        _logger.LogInformation("Inicializando base de datos del sistema DHM...");

        // Tabla de Tenants
        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Tenants (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                ConnectionString TEXT NOT NULL,
                DatabaseProvider INTEGER NOT NULL DEFAULT 0,
                IsActive INTEGER NOT NULL DEFAULT 1,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT
            )");

        // Tabla de HealthTests
        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS HealthTests (
                Id TEXT PRIMARY KEY,
                TenantId TEXT NOT NULL,
                Name TEXT NOT NULL,
                Description TEXT,
                SqlQuery TEXT NOT NULL,
                ExpectedResultType INTEGER NOT NULL DEFAULT 0,
                ExpectedValue TEXT,
                RemediationSql TEXT,
                FrequencySeconds INTEGER NOT NULL DEFAULT 60,
                IsActive INTEGER NOT NULL DEFAULT 1,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT,
                FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE CASCADE
            )");

        // Tabla de ExecutionLogs
        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS ExecutionLogs (
                Id TEXT PRIMARY KEY,
                TestId TEXT NOT NULL,
                TenantId TEXT NOT NULL,
                ExecutedAt TEXT NOT NULL,
                Passed INTEGER NOT NULL DEFAULT 0,
                ResultValue TEXT,
                ErrorMessage TEXT,
                RemediationApplied INTEGER NOT NULL DEFAULT 0,
                DurationMs INTEGER NOT NULL DEFAULT 0,
                FOREIGN KEY (TestId) REFERENCES HealthTests(Id) ON DELETE CASCADE,
                FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE CASCADE
            )");

        // Índices para mejorar performance
        await connection.ExecuteAsync(@"
            CREATE INDEX IF NOT EXISTS IX_HealthTests_TenantId ON HealthTests(TenantId);
            CREATE INDEX IF NOT EXISTS IX_ExecutionLogs_TestId ON ExecutionLogs(TestId);
            CREATE INDEX IF NOT EXISTS IX_ExecutionLogs_TenantId ON ExecutionLogs(TenantId);
            CREATE INDEX IF NOT EXISTS IX_ExecutionLogs_ExecutedAt ON ExecutionLogs(ExecutedAt);
        ");

        _logger.LogInformation("Base de datos del sistema DHM inicializada correctamente.");
    }

    /// <summary>
    /// Crea una base de datos SQLite mock como tenant de demostración con tablas de prueba.
    /// </summary>
    public async Task SeedMockTenantAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        // Verificar si ya existe el tenant demo
        var existingTenant = await connection.QueryFirstOrDefaultAsync<string>(
            "SELECT Id FROM Tenants WHERE Name = 'Demo - Tienda Online'");

        if (existingTenant is not null)
        {
            _logger.LogInformation("El tenant demo ya existe, saltando seed.");
            return;
        }

        _logger.LogInformation("Creando tenant demo con base de datos mock...");

        // Crear la BD mock en un archivo SQLite separado
        var mockDbPath = Path.Combine(AppContext.BaseDirectory, "Data", "mock_tenant.db");
        var mockDir = Path.GetDirectoryName(mockDbPath)!;
        if (!Directory.Exists(mockDir)) Directory.CreateDirectory(mockDir);

        var mockConnectionString = $"Data Source={mockDbPath}";
        using var mockConn = new Microsoft.Data.Sqlite.SqliteConnection(mockConnectionString);
        mockConn.Open();

        // Crear tablas de prueba en la BD mock
        await mockConn.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Productos (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                Precio REAL NOT NULL,
                Stock INTEGER NOT NULL DEFAULT 0,
                Activo INTEGER NOT NULL DEFAULT 1,
                FechaCreacion TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Pedidos (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ClienteNombre TEXT NOT NULL,
                Total REAL NOT NULL,
                Estado TEXT NOT NULL DEFAULT 'Pendiente',
                FechaCreacion TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS PedidoDetalle (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PedidoId INTEGER NOT NULL,
                ProductoId INTEGER NOT NULL,
                Cantidad INTEGER NOT NULL,
                PrecioUnitario REAL NOT NULL,
                FOREIGN KEY (PedidoId) REFERENCES Pedidos(Id),
                FOREIGN KEY (ProductoId) REFERENCES Productos(Id)
            );

            CREATE TABLE IF NOT EXISTS Configuracion (
                Clave TEXT PRIMARY KEY,
                Valor TEXT NOT NULL,
                Descripcion TEXT
            );
        ");

        // Insertar datos de prueba
        var productCount = await mockConn.QuerySingleAsync<int>("SELECT COUNT(*) FROM Productos");
        if (productCount == 0)
        {
            await mockConn.ExecuteAsync(@"
                INSERT INTO Productos (Nombre, Precio, Stock, Activo, FechaCreacion) VALUES
                ('Laptop HP Pavilion', 899.99, 15, 1, datetime('now')),
                ('Mouse Logitech MX', 79.99, 50, 1, datetime('now')),
                ('Teclado Mecánico RGB', 129.99, 30, 1, datetime('now')),
                ('Monitor Samsung 27""', 349.99, 8, 1, datetime('now')),
                ('Auriculares Sony WH', 299.99, 0, 1, datetime('now')),
                ('Webcam Logitech C920', 69.99, 25, 1, datetime('now')),
                ('SSD Samsung 1TB', 109.99, 40, 1, datetime('now')),
                ('RAM Corsair 16GB', 59.99, 60, 1, datetime('now')),
                ('Producto Descontinuado', 19.99, 0, 0, datetime('now', '-30 days')),
                ('Cable HDMI 2m', 12.99, 100, 1, datetime('now'));

                INSERT INTO Pedidos (ClienteNombre, Total, Estado, FechaCreacion) VALUES
                ('Juan Pérez', 979.98, 'Completado', datetime('now', '-5 days')),
                ('María García', 209.98, 'Completado', datetime('now', '-3 days')),
                ('Carlos López', 349.99, 'Enviado', datetime('now', '-1 days')),
                ('Ana Martínez', 129.99, 'Pendiente', datetime('now')),
                ('Pedro Sánchez', 899.99, 'Cancelado', datetime('now', '-7 days'));

                INSERT INTO PedidoDetalle (PedidoId, ProductoId, Cantidad, PrecioUnitario) VALUES
                (1, 1, 1, 899.99),
                (1, 2, 1, 79.99),
                (2, 3, 1, 129.99),
                (2, 2, 1, 79.99),
                (3, 4, 1, 349.99),
                (4, 3, 1, 129.99),
                (5, 1, 1, 899.99);

                INSERT INTO Configuracion (Clave, Valor, Descripcion) VALUES
                ('MAX_STOCK_ALERTA', '5', 'Stock mínimo para generar alerta'),
                ('MONEDA', 'USD', 'Moneda del sistema'),
                ('IVA', '0.21', 'Porcentaje de IVA'),
                ('EMAIL_NOTIFICACIONES', 'admin@tienda.com', 'Email para notificaciones');
            ");
        }

        // Registrar el tenant mock en la BD del sistema
        var tenantId = Guid.NewGuid().ToString();
        await connection.ExecuteAsync(@"
            INSERT INTO Tenants (Id, Name, ConnectionString, DatabaseProvider, IsActive, CreatedAt)
            VALUES (@Id, @Name, @ConnectionString, @DatabaseProvider, 1, @CreatedAt)",
            new
            {
                Id = tenantId,
                Name = "Demo - Tienda Online",
                ConnectionString = mockConnectionString,
                DatabaseProvider = 3, // Sqlite
                CreatedAt = DateTime.UtcNow.ToString("o")
            });

        // Crear tests de ejemplo para el tenant mock
        var tests = new[]
        {
            new {
                Name = "Productos sin stock",
                Description = "Verifica si hay productos activos sin stock (posible problema de inventario)",
                SqlQuery = "SELECT Nombre, Stock FROM Productos WHERE Stock = 0 AND Activo = 1",
                ExpectedResultType = 2, // Exists - Si existen es un problema
                ExpectedValue = (string?)null,
                RemediationSql = (string?)null,
                Frequency = 30
            },
            new {
                Name = "Configuración completa",
                Description = "Verifica que existan todas las configuraciones necesarias del sistema",
                SqlQuery = "SELECT COUNT(*) as Total FROM Configuracion",
                ExpectedResultType = 3, // Count
                ExpectedValue = "4",
                RemediationSql = (string?)null,
                Frequency = 120
            },
            new {
                Name = "Pedidos huérfanos",
                Description = "Busca pedidos sin detalle (inconsistencia de datos)",
                SqlQuery = "SELECT p.Id, p.ClienteNombre FROM Pedidos p LEFT JOIN PedidoDetalle pd ON p.Id = pd.PedidoId WHERE pd.Id IS NULL",
                ExpectedResultType = 2, // Exists - Si existen es un problema
                ExpectedValue = (string?)null,
                RemediationSql = (string?)null,
                Frequency = 60
            },
            new {
                Name = "Productos con precio válido",
                Description = "Verifica que no existan productos con precio negativo o cero",
                SqlQuery = "SELECT COUNT(*) as Total FROM Productos WHERE Precio <= 0",
                ExpectedResultType = 3, // Count
                ExpectedValue = "0",
                RemediationSql = "UPDATE Productos SET Precio = 1.00, Activo = 0 WHERE Precio <= 0",
                Frequency = 60
            },
            new {
                Name = "Integridad de detalle de pedidos",
                Description = "Verifica que todos los detalles de pedido referencien productos existentes",
                SqlQuery = "SELECT pd.Id FROM PedidoDetalle pd LEFT JOIN Productos pr ON pd.ProductoId = pr.Id WHERE pr.Id IS NULL",
                ExpectedResultType = 3, // Count (esperamos 0)
                ExpectedValue = "0",
                RemediationSql = (string?)null,
                Frequency = 90
            }
        };

        foreach (var test in tests)
        {
            await connection.ExecuteAsync(@"
                INSERT INTO HealthTests (Id, TenantId, Name, Description, SqlQuery, ExpectedResultType, ExpectedValue, RemediationSql, FrequencySeconds, IsActive, CreatedAt)
                VALUES (@Id, @TenantId, @Name, @Description, @SqlQuery, @ExpectedResultType, @ExpectedValue, @RemediationSql, @FrequencySeconds, 1, @CreatedAt)",
                new
                {
                    Id = Guid.NewGuid().ToString(),
                    TenantId = tenantId,
                    test.Name,
                    test.Description,
                    test.SqlQuery,
                    test.ExpectedResultType,
                    test.ExpectedValue,
                    test.RemediationSql,
                    FrequencySeconds = test.Frequency,
                    CreatedAt = DateTime.UtcNow.ToString("o")
                });
        }

        _logger.LogInformation("Tenant demo 'Tienda Online' creado con {TestCount} tests de prueba.", tests.Length);
    }
}
