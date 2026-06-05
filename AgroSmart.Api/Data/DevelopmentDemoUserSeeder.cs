using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AgroSmart.Api.Models;
using AgroSmart.Api.Repositories;

namespace AgroSmart.Api.Data;

/// <summary>
/// Seeds Development-only demo data so the platform is immediately usable for
/// Swagger/Postman testing and so the Kafka streaming pipeline has registered
/// devices and alert rules to feed into. All operations are idempotent.
/// </summary>
public static class DevelopmentDemoUserSeeder
{
    public const string DemoEmail = "operador@agrosmart.com.br";
    public const string DemoPassword = "agrosmart123";

    public static async Task SeedAsync(IServiceProvider services)
    {
        await SeedDemoUserAsync(services);
        await SeedAgronomicDataAsync(services);
    }

    private static async Task SeedDemoUserAsync(IServiceProvider services)
    {
        var users = services.GetRequiredService<IUserRepository>();
        var email = DemoEmail.Trim().ToLowerInvariant();

        if (await users.ExistsAsync(u => u.Email == email))
            return;

        var hasher = new PasswordHasher<User>();
        var user = new User
        {
            Email = email,
            Role = "Operator",
            PasswordHash = string.Empty
        };
        user.PasswordHash = hasher.HashPassword(user, DemoPassword);

        await users.AddAsync(user);
        await users.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds fields (regions), field sensors (devices) and alert rules used by the
    /// continuous data pipeline. The simulated device identifiers must already exist
    /// for Kafka ingestion to resolve them.
    /// </summary>
    private static async Task SeedAgronomicDataAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();

        if (await db.Regions.AnyAsync())
            return; // demo data already present

        var fields = new[]
        {
            new Region { Code = "TALHAO-01", Name = "Talhão Norte - Soja",       ModuleType = "Sequeiro",  FieldLocation = "Fazenda Boa Vista / Setor Norte", Description = "Talhão de soja em sistema de sequeiro." },
            new Region { Code = "TALHAO-02", Name = "Talhão Sul - Milho",        ModuleType = "Irrigado",  FieldLocation = "Fazenda Boa Vista / Setor Sul",   Description = "Talhão de milho com pivô central." },
            new Region { Code = "ESTUFA-01", Name = "Estufa 01 - Hortaliças",    ModuleType = "Estufa",    FieldLocation = "Fazenda Boa Vista / Estufas",     Description = "Estufa de hortaliças folhosas com controle climático." }
        };
        db.Regions.AddRange(fields);
        await db.SaveChangesAsync();

        Region Field(string code) => fields.First(f => f.Code == code);

        var devices = new[]
        {
            new Device { Identifier = "SENSOR-T1-01", Name = "Estação de Campo T1-01", DeviceType = "MultiSensor", Status = DeviceStatus.Active, FirmwareVersion = "2.1.0", RegionId = Field("TALHAO-01").Id },
            new Device { Identifier = "SENSOR-T2-01", Name = "Estação de Campo T2-01", DeviceType = "MultiSensor", Status = DeviceStatus.Active, FirmwareVersion = "2.1.0", RegionId = Field("TALHAO-02").Id },
            new Device { Identifier = "SENSOR-E1-01", Name = "Sonda de Estufa E1-01",  DeviceType = "Soil Probe",  Status = DeviceStatus.Active, FirmwareVersion = "2.0.3", RegionId = Field("ESTUFA-01").Id }
        };
        db.Devices.AddRange(devices);
        await db.SaveChangesAsync();

        var metricByCode = await db.MetricTypes.ToDictionaryAsync(m => m.Code, m => m.Id);

        var rules = new List<AlertRule>
        {
            new() { Name = "Temperatura alta (global)",   Description = "Temperatura do ar acima do ideal para a cultura.", MetricTypeId = metricByCode["TEMPERATURE"],   MaxThreshold = 32, Severity = AlertSeverity.Warning,  IsActive = true },
            new() { Name = "Umidade do ar baixa (global)", Description = "Umidade relativa do ar muito baixa.",              MetricTypeId = metricByCode["HUMIDITY"],      MinThreshold = 40, Severity = AlertSeverity.Warning,  IsActive = true },
            new() { Name = "Umidade do solo crítica",      Description = "Umidade do solo abaixo do nível seguro (estresse hídrico).", MetricTypeId = metricByCode["SOIL_MOISTURE"], MinThreshold = 35, Severity = AlertSeverity.Critical, IsActive = true },
            new() { Name = "pH do solo fora de faixa",     Description = "Acidez do solo fora da faixa agronômica ideal.",   MetricTypeId = metricByCode["PH"],            MinThreshold = 5.5, MaxThreshold = 6.8, Severity = AlertSeverity.Warning, IsActive = true }
        };
        db.AlertRules.AddRange(rules);
        await db.SaveChangesAsync();
    }
}
