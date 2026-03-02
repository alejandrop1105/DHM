using DHM.Infrastructure;
using DHM.Infrastructure.Data;
using DHM.Web.Components;
using DHM.Web.Hubs;
using DHM.Web.Workers;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar SQLite del sistema
var dbPath = Path.Combine(AppContext.BaseDirectory, "Data", "dhm_system.db");
var dataDir = Path.GetDirectoryName(dbPath)!;
if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
var systemConnectionString = $"Data Source={dbPath}";

// Registrar servicios de infraestructura (repositorios, servicios, DI)
builder.Services.AddInfrastructure(systemConnectionString);

// MudBlazor
builder.Services.AddMudServices();

// SignalR
builder.Services.AddSignalR();

// Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Background Worker
builder.Services.AddHostedService<HealthCheckWorker>();

var app = builder.Build();

// Inicializar la base de datos y seed de tenant mock
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync();
    await initializer.SeedMockTenantAsync();
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map SignalR Hub
app.MapHub<MonitorHub>("/monitorhub");

app.Run();
