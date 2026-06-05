using Xunit.Abstractions;

namespace AgroSmart.Api.Tests.Support;

/// <summary>Writes ASCII scenario banners to console, test output, and optional log file.</summary>
public sealed class TestScenarioLogger
{
    private const int Width = 64;
    private static readonly string? LogFilePath = ResolveLogFilePath();
    private readonly ITestOutputHelper? _output;

    public TestScenarioLogger(ITestOutputHelper? output = null) => _output = output;

    public void Begin(string scenarioId, string title)
    {
        var border = new string('=', Width);
        Write("");
        Write(border);
        Write($"  {scenarioId} | {title}");
        Write(border);
    }

    public void Step(string message) => Write($"  >> {message}");

    public void Data(string key, object? value) => Write($"     {key,-12}: {value}");

    public void Pass(string summary)
    {
        Write($"  [OK] RESULTADO : {summary}");
        Write($"  [OK] STATUS    : PASSED");
        Write("");
    }

    public void Fail(string summary)
    {
        Write($"  [!!] RESULTADO : {summary}");
        Write($"  [!!] STATUS    : FAILED");
        Write("");
    }

    private static string? ResolveLogFilePath()
    {
        var dir = Environment.GetEnvironmentVariable("ORBITAL_TEST_LOG_DIR");
        if (string.IsNullOrWhiteSpace(dir))
            return null;

        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "unit-test-scenarios.log");
    }

    private void Write(string text)
    {
        try
        {
            Console.Out.WriteLine(text);
            Console.Out.Flush();
        }
        catch
        {
            // ignored in some test hosts
        }

        _output?.WriteLine(text);

        if (LogFilePath is null)
            return;

        try
        {
            File.AppendAllText(LogFilePath, text + Environment.NewLine);
        }
        catch
        {
            // ignored if log dir not writable
        }
    }
}
