using DHM.Application.Interfaces;
using DHM.Application.Services;
using DHM.Domain.Interfaces;
using DHM.Infrastructure.Data;
using DHM.Infrastructure.Repositories;
using DHM.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DHM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Registrar el TypeHandler para que Dapper convierta Guid <-> string en SQLite
        Dapper.SqlMapper.AddTypeHandler(new SqliteGuidTypeHandler());

        // Data
        var factory = new SqliteConnectionFactory(connectionString);
        services.AddSingleton(factory);
        services.AddSingleton<DatabaseInitializer>();

        // Repositorios
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IHealthTestRepository, HealthTestRepository>();
        services.AddScoped<IExecutionLogRepository, ExecutionLogRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ITagGroupRepository, TagGroupRepository>();
        services.AddScoped<ISavedQueryRepository, SavedQueryRepository>();

        // Servicios de infraestructura
        services.AddScoped<IExternalDatabaseService, ExternalDatabaseService>();

        // Servicios de aplicación
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IHealthTestService, HealthTestService>();
        services.AddScoped<ITestExecutionService, TestExecutionService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<ITagGroupService, TagGroupService>();
        services.AddScoped<ISavedQueryService, SavedQueryService>();
        services.AddScoped<ISqlExecutionService, SqlExecutionService>();

        return services;
    }
}
