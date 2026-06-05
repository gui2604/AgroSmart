using System.Globalization;
using AgroSmart.Api.Models;

namespace AgroSmart.Api.Services;

/// <summary>Evaluates a measurement against an alert rule min/max thresholds.</summary>
public static class AlertThresholdEvaluator
{
    public static (bool Violated, string Reason) Evaluate(AlertRule rule, double value)
    {
        if (rule.MinThreshold.HasValue && value < rule.MinThreshold.Value)
            return (true, $"abaixo do mínimo de {rule.MinThreshold.Value.ToString(CultureInfo.InvariantCulture)}");

        if (rule.MaxThreshold.HasValue && value > rule.MaxThreshold.Value)
            return (true, $"acima do máximo de {rule.MaxThreshold.Value.ToString(CultureInfo.InvariantCulture)}");

        return (false, string.Empty);
    }
}
