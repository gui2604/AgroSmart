using Microsoft.EntityFrameworkCore;

namespace AgroSmart.Api.Data;

/// <summary>
/// Applies EF migrations and demo seed data in Development. Recovers from a
/// corrupted Oracle schema (orphan migration history or partial tables).
/// </summary>
public static class DevelopmentDatabaseInitializer
{
    private const string InitialMigrationId = "20260604135925_InitialCreate";

    public static async Task InitializeAsync(
        ApplicationDbContext db,
        IServiceProvider services,
        ILogger logger)
    {
        if (!await ApplicationTablesExistAsync(db))
        {
            logger.LogWarning(
                "Tabelas AGS_* ausentes. Limpando histórico EF órfão e reaplicando a migration inicial.");
            await ClearInitialMigrationHistoryAsync(db);
        }

        if (!TryMigrate(db, logger))
        {
            logger.LogWarning(
                "Schema Oracle inconsistente (constraints/tabelas parciais). Recriando tabelas AGS_*...");
            await DropAgroSmartSchemaAsync(db, logger);
            await ClearInitialMigrationHistoryAsync(db);

            if (!TryMigrate(db, logger))
                throw new InvalidOperationException(
                    "Não foi possível aplicar as migrations do AgroSmart no Oracle. " +
                    "Execute db/00_drop_ags_schema.sql no SQL Developer e reinicie a API.");
        }

        await DevelopmentDemoUserSeeder.SeedAsync(services);
    }

    private static bool TryMigrate(ApplicationDbContext db, ILogger logger)
    {
        try
        {
            db.Database.Migrate();
            return true;
        }
        catch (Exception ex) when (IsSchemaConflict(ex))
        {
            logger.LogWarning(ex, "Conflito ao aplicar migration (schema parcial).");
            return false;
        }
    }

    private static async Task<bool> ApplicationTablesExistAsync(ApplicationDbContext db)
    {
        try
        {
            await db.Database.ExecuteSqlRawAsync("SELECT 1 FROM AGS_USERS WHERE 1 = 0");
            return true;
        }
        catch (Exception ex) when (IsMissingTable(ex))
        {
            return false;
        }
    }

    private static async Task ClearInitialMigrationHistoryAsync(ApplicationDbContext db)
    {
        try
        {
            await db.Database.ExecuteSqlRawAsync(
                $"""DELETE FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'{InitialMigrationId}'""");
        }
        catch (Exception ex) when (IsMissingTable(ex))
        {
            // History table not created yet; Migrate() will create everything.
        }
    }

    private static async Task DropAgroSmartSchemaAsync(ApplicationDbContext db, ILogger logger)
    {
        const string dropScript = """
            BEGIN
                FOR t IN (SELECT table_name FROM user_tables WHERE table_name LIKE 'AGS\_%' ESCAPE '\') LOOP
                    EXECUTE IMMEDIATE 'DROP TABLE "' || t.table_name || '" CASCADE CONSTRAINTS PURGE';
                END LOOP;
            END;
            """;

        try
        {
            await db.Database.ExecuteSqlRawAsync(dropScript);
            logger.LogInformation("Tabelas AGS_* removidas para recriação limpa.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao remover tabelas AGS_*.");
            throw;
        }
    }

    private static bool IsMissingTable(Exception ex)
        => ContainsOracleError(ex, "ORA-00942");

    private static bool IsSchemaConflict(Exception ex)
        => ContainsOracleError(ex, "ORA-02264", "ORA-00955", "ORA-01418");

    private static bool ContainsOracleError(Exception ex, params string[] codes)
    {
        for (var current = ex; current is not null; current = current.InnerException)
        {
            foreach (var code in codes)
            {
                if (current.Message.Contains(code, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        return false;
    }
}
