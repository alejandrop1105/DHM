using System.Diagnostics;
using DHM.Application.DTOs;
using DHM.Application.Interfaces;
using DHM.Domain.Entities;
using DHM.Domain.Enums;
using DHM.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DHM.Application.Services;

public class TestExecutionService : ITestExecutionService
{
    private readonly IHealthTestRepository _testRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IExecutionLogRepository _logRepository;
    private readonly IExternalDatabaseService _externalDbService;
    private readonly ILogger<TestExecutionService> _logger;

    public TestExecutionService(
        IHealthTestRepository testRepository,
        ITenantRepository tenantRepository,
        IExecutionLogRepository logRepository,
        IExternalDatabaseService externalDbService,
        ILogger<TestExecutionService> logger)
    {
        _testRepository = testRepository;
        _tenantRepository = tenantRepository;
        _logRepository = logRepository;
        _externalDbService = externalDbService;
        _logger = logger;
    }

    public async Task<ExecutionLogDto> ExecuteTestAsync(Guid testId)
    {
        var test = await _testRepository.GetByIdAsync(testId)
            ?? throw new InvalidOperationException($"Test {testId} no encontrado.");

        var tenant = await _tenantRepository.GetByIdAsync(test.TenantId)
            ?? throw new InvalidOperationException($"Tenant {test.TenantId} no encontrado.");

        var sw = Stopwatch.StartNew();
        var log = new ExecutionLog
        {
            Id = Guid.NewGuid(),
            TestId = test.Id,
            TenantId = tenant.Id,
            ExecutedAt = DateTime.UtcNow
        };

        try
        {
            // Ejecutar la query de test contra la BD externa del tenant
            var results = (await _externalDbService.ExecuteQueryAsync(
                tenant.ConnectionString, tenant.DatabaseProvider, test.SqlQuery)).ToList();

            // Evaluar el resultado
            log.Passed = EvaluateResult(test, results, out string resultValue);
            log.ResultValue = resultValue;

            // Self-Healing: si falla y hay SQL de remediación
            if (!log.Passed && !string.IsNullOrWhiteSpace(test.RemediationSql))
            {
                try
                {
                    await _externalDbService.ExecuteCommandAsync(
                        tenant.ConnectionString, tenant.DatabaseProvider, test.RemediationSql);
                    log.RemediationApplied = true;
                    _logger.LogInformation("Remediación aplicada para test '{TestName}' del tenant '{TenantName}'",
                        test.Name, tenant.Name);
                }
                catch (Exception remEx)
                {
                    _logger.LogWarning(remEx, "Error al aplicar remediación para test '{TestName}'", test.Name);
                    log.ErrorMessage = $"{log.ErrorMessage} | Remediación fallida: {remEx.Message}";
                }
            }
        }
        catch (Exception ex)
        {
            log.Passed = false;
            log.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error al ejecutar test '{TestName}' para tenant '{TenantName}'",
                test.Name, tenant.Name);
        }

        sw.Stop();
        log.DurationMs = sw.ElapsedMilliseconds;

        await _logRepository.CreateAsync(log);

        return new ExecutionLogDto
        {
            Id = log.Id,
            TestId = log.TestId,
            TestName = test.Name,
            TenantId = log.TenantId,
            TenantName = tenant.Name,
            ExecutedAt = log.ExecutedAt,
            Passed = log.Passed,
            ResultValue = log.ResultValue,
            ErrorMessage = log.ErrorMessage,
            RemediationApplied = log.RemediationApplied,
            DurationMs = log.DurationMs
        };
    }

    public async Task<IEnumerable<ExecutionLogDto>> ExecuteAllPendingTestsAsync()
    {
        var results = new List<ExecutionLogDto>();
        var tests = await _testRepository.GetActiveTestsDueForExecutionAsync();

        foreach (var test in tests)
        {
            try
            {
                var result = await ExecuteTestAsync(test.Id);
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ejecutar test pendiente {TestId}", test.Id);
            }
        }

        return results;
    }

    /// <summary>
    /// Evalúa si el resultado de la query cumple con las expectativas del test.
    /// </summary>
    private static bool EvaluateResult(HealthTest test, List<dynamic> results, out string resultValue)
    {
        switch (test.ExpectedResultType)
        {
            case ExpectedResultType.Exists:
                resultValue = $"{results.Count} fila(s)";
                return results.Count > 0;

            case ExpectedResultType.Count:
                var countValue = results.Count.ToString();
                if (results.Count == 1)
                {
                    var firstRow = (IDictionary<string, object>)results[0];
                    var firstVal = firstRow.Values.FirstOrDefault();
                    if (firstVal != null)
                        countValue = firstVal.ToString() ?? countValue;
                }
                resultValue = countValue;
                return string.Equals(countValue, test.ExpectedValue, StringComparison.OrdinalIgnoreCase);

            case ExpectedResultType.UniqueValue:
                if (results.Count != 1)
                {
                    resultValue = $"Se esperaba 1 fila, se obtuvieron {results.Count}";
                    return false;
                }
                var row = (IDictionary<string, object>)results[0];
                var val = row.Values.FirstOrDefault()?.ToString() ?? "";
                resultValue = val;
                return string.Equals(val, test.ExpectedValue, StringComparison.OrdinalIgnoreCase);

            case ExpectedResultType.List:
                resultValue = $"{results.Count} fila(s)";
                return results.Count > 0;

            default:
                resultValue = "Tipo de resultado no soportado";
                return false;
        }
    }
}
