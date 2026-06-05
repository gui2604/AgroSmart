namespace AgroSmart.Api.Models;

/// <summary>Operational status of a field device/sensor.</summary>
public enum DeviceStatus
{
    Active,
    Inactive,
    Maintenance,
    Faulty
}

/// <summary>Severity associated with an alert rule and the alerts it produces.</summary>
public enum AlertSeverity
{
    Info,
    Warning,
    Critical
}

/// <summary>Lifecycle status of a triggered alert.</summary>
public enum AlertStatus
{
    Open,
    Acknowledged,
    Resolved
}
