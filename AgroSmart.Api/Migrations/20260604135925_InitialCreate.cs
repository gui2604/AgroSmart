using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AgroSmart.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AGS_METRIC_TYPES",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    Unit = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: true),
                    NominalMin = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    NominalMax = table.Column<double>(type: "BINARY_DOUBLE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_METRIC_TYPE", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AGS_REGIONS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    ModuleType = table.Column<string>(type: "NVARCHAR2(60)", maxLength: 60, nullable: true),
                    FieldLocation = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: true),
                    Description = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REGION", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AGS_USERS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Email = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: false),
                    Role = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AGS_ALERT_RULES",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: true),
                    MetricTypeId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    RegionId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    MinThreshold = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    MaxThreshold = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    Severity = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<int>(type: "NUMBER(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ALERT_RULE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RULE_METRIC",
                        column: x => x.MetricTypeId,
                        principalTable: "AGS_METRIC_TYPES",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RULE_REGION",
                        column: x => x.RegionId,
                        principalTable: "AGS_REGIONS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AGS_DEVICES",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Identifier = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    DeviceType = table.Column<string>(type: "NVARCHAR2(60)", maxLength: 60, nullable: true),
                    Status = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    FirmwareVersion = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: true),
                    InstalledAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    RegionId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DEVICE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DEVICE_REGION",
                        column: x => x.RegionId,
                        principalTable: "AGS_REGIONS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AGS_SENSOR_READINGS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CollectedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    SourceFile = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: true),
                    DeviceId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_READING", x => x.Id);
                    table.ForeignKey(
                        name: "FK_READING_DEVICE",
                        column: x => x.DeviceId,
                        principalTable: "AGS_DEVICES",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AGS_MEASUREMENTS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Value = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    SensorReadingId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MetricTypeId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MEASUREMENT", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MEAS_METRIC",
                        column: x => x.MetricTypeId,
                        principalTable: "AGS_METRIC_TYPES",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MEAS_READING",
                        column: x => x.SensorReadingId,
                        principalTable: "AGS_SENSOR_READINGS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AGS_ALERTS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Message = table.Column<string>(type: "NVARCHAR2(400)", maxLength: 400, nullable: false),
                    Severity = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    TriggeredValue = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    AlertRuleId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    MetricTypeId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MeasurementId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    DeviceId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    RegionId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    AcknowledgedByUserId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ALERT", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ALERT_DEVICE",
                        column: x => x.DeviceId,
                        principalTable: "AGS_DEVICES",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALERT_MEAS",
                        column: x => x.MeasurementId,
                        principalTable: "AGS_MEASUREMENTS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ALERT_METRIC",
                        column: x => x.MetricTypeId,
                        principalTable: "AGS_METRIC_TYPES",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALERT_REGION",
                        column: x => x.RegionId,
                        principalTable: "AGS_REGIONS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALERT_RULE",
                        column: x => x.AlertRuleId,
                        principalTable: "AGS_ALERT_RULES",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALERT_USER",
                        column: x => x.AcknowledgedByUserId,
                        principalTable: "AGS_USERS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "AGS_METRIC_TYPES",
                columns: new[] { "Id", "Code", "Description", "Name", "NominalMax", "NominalMin", "Unit" },
                values: new object[,]
                {
                    { 1, "TEMPERATURE", "Ambient air temperature inside the growth module.", "Air Temperature", 26.0, 18.0, "C" },
                    { 2, "HUMIDITY", "Relative air humidity.", "Relative Humidity", 80.0, 50.0, "%" },
                    { 3, "CO2", "Carbon dioxide concentration available for photosynthesis.", "Carbon Dioxide", 1200.0, 400.0, "ppm" },
                    { 4, "O2", "Oxygen concentration in the cabin atmosphere.", "Oxygen", 23.0, 19.0, "%" },
                    { 5, "LUMINOSITY", "Photosynthetically active radiation reaching the canopy.", "Photosynthetic Light", 800.0, 200.0, "umol/m2/s" },
                    { 6, "SOIL_MOISTURE", "Water content of the growing substrate.", "Substrate Moisture", 70.0, 40.0, "%" },
                    { 7, "PH", "Acidity of the hydroponic nutrient solution.", "Nutrient Solution pH", 6.5, 5.5, "pH" },
                    { 8, "EC", "Electrical conductivity (nutrient strength) of the solution.", "Nutrient Conductivity", 2.3999999999999999, 1.2, "mS/cm" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AGS_ALERT_RULES_MetricTypeId",
                table: "AGS_ALERT_RULES",
                column: "MetricTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AGS_ALERT_RULES_RegionId",
                table: "AGS_ALERT_RULES",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_ALERT_STATUS",
                table: "AGS_ALERTS",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AGS_ALERTS_AcknowledgedByUserId",
                table: "AGS_ALERTS",
                column: "AcknowledgedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AGS_ALERTS_AlertRuleId",
                table: "AGS_ALERTS",
                column: "AlertRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_AGS_ALERTS_DeviceId",
                table: "AGS_ALERTS",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_AGS_ALERTS_MeasurementId",
                table: "AGS_ALERTS",
                column: "MeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_AGS_ALERTS_MetricTypeId",
                table: "AGS_ALERTS",
                column: "MetricTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AGS_ALERTS_RegionId",
                table: "AGS_ALERTS",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_AGS_DEVICES_RegionId",
                table: "AGS_DEVICES",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "UX_DEVICE_IDENT",
                table: "AGS_DEVICES",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MEAS_METRIC",
                table: "AGS_MEASUREMENTS",
                column: "MetricTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MEAS_READING",
                table: "AGS_MEASUREMENTS",
                column: "SensorReadingId");

            migrationBuilder.CreateIndex(
                name: "UX_METRIC_CODE",
                table: "AGS_METRIC_TYPES",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_REGION_CODE",
                table: "AGS_REGIONS",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_READING_DEVICE",
                table: "AGS_SENSOR_READINGS",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "UX_USER_EMAIL",
                table: "AGS_USERS",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AGS_ALERTS");

            migrationBuilder.DropTable(
                name: "AGS_MEASUREMENTS");

            migrationBuilder.DropTable(
                name: "AGS_ALERT_RULES");

            migrationBuilder.DropTable(
                name: "AGS_USERS");

            migrationBuilder.DropTable(
                name: "AGS_SENSOR_READINGS");

            migrationBuilder.DropTable(
                name: "AGS_METRIC_TYPES");

            migrationBuilder.DropTable(
                name: "AGS_DEVICES");

            migrationBuilder.DropTable(
                name: "AGS_REGIONS");
        }
    }
}
