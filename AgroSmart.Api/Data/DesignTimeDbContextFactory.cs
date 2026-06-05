using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AgroSmart.Api.Data;

/// <summary>
/// Enables <c>dotnet ef</c> commands (migrations) to construct the context at design time.
/// Generating Oracle migrations does not require a live database connection.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("appsettings.Development.local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("OracleDb")
            ?? "User Id=agrosmart;Password=placeholder;Data Source=localhost:1521/ORCL";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseOracle(connectionString)
            .Options;

        return new ApplicationDbContext(options);
    }
}
