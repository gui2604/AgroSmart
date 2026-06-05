using AgroSmart.Api.Models;
using AgroSmart.Api.Services;
using AgroSmart.Api.Tests.Support;
using Xunit;
using Xunit.Abstractions;

namespace AgroSmart.Api.Tests.Services;

public class AlertThresholdEvaluatorTests
{
    private readonly TestScenarioLogger _log;

    public AlertThresholdEvaluatorTests(ITestOutputHelper output) => _log = new TestScenarioLogger(output);

    [Fact(DisplayName = "CT-01 — CO2 dentro da faixa nominal não dispara alerta")]
    public void CT01_Co2WithinRange_DoesNotViolate()
    {
        _log.Begin("CT-01", "CO2 dentro da faixa — sem violação");
        var rule = new AlertRule { Name = "CO2 máx", MaxThreshold = 1200 };
        const double value = 850;

        _log.Data("Métrica", "CO2");
        _log.Data("Valor", $"{value} ppm");
        _log.Data("Limiar máx", rule.MaxThreshold);

        var (violated, reason) = AlertThresholdEvaluator.Evaluate(rule, value);

        _log.Step($"Violou regra? {violated}");
        Assert.False(violated);
        Assert.Empty(reason);
        _log.Pass("Medição nominal; nenhum alerta seria gerado na ingestão.");
    }

    [Fact(DisplayName = "CT-02 — CO2 acima do máximo dispara violação")]
    public void CT02_Co2AboveMax_ViolatesWithReason()
    {
        _log.Begin("CT-02", "CO2 acima do máximo — violação");
        var rule = new AlertRule { Name = "CO2 crítico", MaxThreshold = 1200 };
        const double value = 1850;

        _log.Data("Métrica", "CO2");
        _log.Data("Valor", $"{value} ppm");
        _log.Data("Limiar máx", rule.MaxThreshold);

        var (violated, reason) = AlertThresholdEvaluator.Evaluate(rule, value);

        _log.Step($"Violou regra? {violated}");
        _log.Data("Motivo", reason);
        Assert.True(violated);
        Assert.Contains("acima do máximo", reason);
        _log.Pass("Ingestão geraria alerta automático (ex.: leitura SENSOR-A1-02).");
    }

    [Fact(DisplayName = "CT-03 — O2 abaixo do mínimo dispara violação")]
    public void CT03_O2BelowMin_ViolatesWithReason()
    {
        _log.Begin("CT-03", "O2 abaixo do mínimo — violação");
        var rule = new AlertRule { Name = "O2 mínimo", MinThreshold = 19 };
        const double value = 17.8;

        _log.Data("Métrica", "O2");
        _log.Data("Valor", $"{value} %");
        _log.Data("Limiar mín", rule.MinThreshold);

        var (violated, reason) = AlertThresholdEvaluator.Evaluate(rule, value);

        _log.Step($"Violou regra? {violated}");
        _log.Data("Motivo", reason);
        Assert.True(violated);
        Assert.Contains("abaixo do mínimo", reason);
        _log.Pass("Regra de oxigênio identifica atmosfera crítica na baia.");
    }
}
