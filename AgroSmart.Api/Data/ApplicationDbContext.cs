using Microsoft.EntityFrameworkCore;
using AgroSmart.Api.Models;

namespace AgroSmart.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Region> Regions => Set<Region>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<MetricType> MetricTypes => Set<MetricType>();
    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();
    public DbSet<Measurement> Measurements => Set<Measurement>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Constraint/index names are prefixed with AGS_ so the schema can coexist
        // with other FIAP projects in the same Oracle user (shared schema).

        b.Entity<Region>(e =>
        {
            e.ToTable("AGS_REGIONS");
            e.HasKey(x => x.Id).HasName("AGS_PK_REGION");
            e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("AGS_UX_REGION_CODE");
        });

        b.Entity<MetricType>(e =>
        {
            e.ToTable("AGS_METRIC_TYPES");
            e.HasKey(x => x.Id).HasName("AGS_PK_METRIC");
            e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("AGS_UX_METRIC_CODE");
        });

        b.Entity<Device>(e =>
        {
            e.ToTable("AGS_DEVICES");
            e.HasKey(x => x.Id).HasName("AGS_PK_DEVICE");
            e.HasIndex(x => x.Identifier).IsUnique().HasDatabaseName("AGS_UX_DEVICE_IDENT");
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

            e.HasOne(x => x.Region)
                .WithMany(r => r.Devices)
                .HasForeignKey(x => x.RegionId)
                .HasConstraintName("AGS_FK_DEVICE_REGION")
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<SensorReading>(e =>
        {
            e.ToTable("AGS_SENSOR_READINGS");
            e.HasKey(x => x.Id).HasName("AGS_PK_READING");
            e.HasIndex(x => x.DeviceId).HasDatabaseName("AGS_IX_READING_DEVICE");

            e.HasOne(x => x.Device)
                .WithMany(d => d.Readings)
                .HasForeignKey(x => x.DeviceId)
                .HasConstraintName("AGS_FK_READING_DEVICE")
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Measurement>(e =>
        {
            e.ToTable("AGS_MEASUREMENTS");
            e.HasKey(x => x.Id).HasName("AGS_PK_MEASUREMENT");
            e.HasIndex(x => x.SensorReadingId).HasDatabaseName("AGS_IX_MEAS_READING");
            e.HasIndex(x => x.MetricTypeId).HasDatabaseName("AGS_IX_MEAS_METRIC");

            e.HasOne(x => x.SensorReading)
                .WithMany(r => r.Measurements)
                .HasForeignKey(x => x.SensorReadingId)
                .HasConstraintName("AGS_FK_MEAS_READING")
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.MetricType)
                .WithMany(m => m.Measurements)
                .HasForeignKey(x => x.MetricTypeId)
                .HasConstraintName("AGS_FK_MEAS_METRIC")
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<AlertRule>(e =>
        {
            e.ToTable("AGS_ALERT_RULES");
            e.HasKey(x => x.Id).HasName("AGS_PK_ALERT_RULE");
            e.Property(x => x.Severity).HasConversion<string>().HasMaxLength(20);

            // Map bool to NUMBER(1) for portability (Oracle < 23ai has no native BOOLEAN type).
            e.Property(x => x.IsActive)
                .HasColumnType("NUMBER(1)")
                .HasConversion<int>();

            e.HasOne(x => x.MetricType)
                .WithMany(m => m.AlertRules)
                .HasForeignKey(x => x.MetricTypeId)
                .HasConstraintName("AGS_FK_RULE_METRIC")
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Region)
                .WithMany(r => r.AlertRules)
                .HasForeignKey(x => x.RegionId)
                .HasConstraintName("AGS_FK_RULE_REGION")
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Alert>(e =>
        {
            e.ToTable("AGS_ALERTS");
            e.HasKey(x => x.Id).HasName("AGS_PK_ALERT");
            e.Property(x => x.Severity).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.Status).HasDatabaseName("AGS_IX_ALERT_STATUS");

            // All alert relationships use Restrict to avoid multiple cascade paths,
            // which Oracle does not allow.
            e.HasOne(x => x.AlertRule)
                .WithMany(r => r.Alerts)
                .HasForeignKey(x => x.AlertRuleId)
                .HasConstraintName("AGS_FK_ALERT_RULE")
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.MetricType)
                .WithMany()
                .HasForeignKey(x => x.MetricTypeId)
                .HasConstraintName("AGS_FK_ALERT_METRIC")
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Measurement)
                .WithMany(m => m.Alerts)
                .HasForeignKey(x => x.MeasurementId)
                .HasConstraintName("AGS_FK_ALERT_MEAS")
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(x => x.Device)
                .WithMany(d => d.Alerts)
                .HasForeignKey(x => x.DeviceId)
                .HasConstraintName("AGS_FK_ALERT_DEVICE")
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Region)
                .WithMany(r => r.Alerts)
                .HasForeignKey(x => x.RegionId)
                .HasConstraintName("AGS_FK_ALERT_REGION")
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.AcknowledgedByUser)
                .WithMany(u => u.AcknowledgedAlerts)
                .HasForeignKey(x => x.AcknowledgedByUserId)
                .HasConstraintName("AGS_FK_ALERT_USER")
                .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<User>(e =>
        {
            e.ToTable("AGS_USERS");
            e.HasKey(x => x.Id).HasName("AGS_PK_USER");
            e.HasIndex(x => x.Email).IsUnique().HasDatabaseName("AGS_UX_USER_EMAIL");
        });

        SeedMetricTypes(b);
    }

    /// <summary>Seeds the catalog of environmental metrics tracked by the greenhouse.</summary>
    private static void SeedMetricTypes(ModelBuilder b)
    {
        b.Entity<MetricType>().HasData(
            new MetricType { Id = 1, Code = "TEMPERATURE", Name = "Air Temperature", Unit = "C", NominalMin = 18, NominalMax = 26, Description = "Ambient air temperature inside the growth module." },
            new MetricType { Id = 2, Code = "HUMIDITY", Name = "Relative Humidity", Unit = "%", NominalMin = 50, NominalMax = 80, Description = "Relative air humidity." },
            new MetricType { Id = 3, Code = "CO2", Name = "Carbon Dioxide", Unit = "ppm", NominalMin = 400, NominalMax = 1200, Description = "Carbon dioxide concentration available for photosynthesis." },
            new MetricType { Id = 4, Code = "O2", Name = "Oxygen", Unit = "%", NominalMin = 19, NominalMax = 23, Description = "Oxygen concentration in the cabin atmosphere." },
            new MetricType { Id = 5, Code = "LUMINOSITY", Name = "Photosynthetic Light", Unit = "umol/m2/s", NominalMin = 200, NominalMax = 800, Description = "Photosynthetically active radiation reaching the canopy." },
            new MetricType { Id = 6, Code = "SOIL_MOISTURE", Name = "Substrate Moisture", Unit = "%", NominalMin = 40, NominalMax = 70, Description = "Water content of the growing substrate." },
            new MetricType { Id = 7, Code = "PH", Name = "Nutrient Solution pH", Unit = "pH", NominalMin = 5.5, NominalMax = 6.5, Description = "Acidity of the hydroponic nutrient solution." },
            new MetricType { Id = 8, Code = "EC", Name = "Nutrient Conductivity", Unit = "mS/cm", NominalMin = 1.2, NominalMax = 2.4, Description = "Electrical conductivity (nutrient strength) of the solution." }
        );
    }
}
