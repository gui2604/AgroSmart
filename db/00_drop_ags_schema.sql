-- ============================================================================
--  AgroSmart - Reset do schema (Development / recuperação de erros EF)
--  Remove todas as tabelas AGS_* e o histórico da migration inicial.
--  Execute no SQL Developer conectado ao seu schema FIAP antes de subir a API.
-- ============================================================================

BEGIN
    FOR t IN (
        SELECT table_name
        FROM user_tables
        WHERE table_name LIKE 'AGS\_%' ESCAPE '\'
    ) LOOP
        EXECUTE IMMEDIATE 'DROP TABLE "' || t.table_name || '" CASCADE CONSTRAINTS PURGE';
    END LOOP;
END;
/

DELETE FROM "__EFMigrationsHistory"
 WHERE "MigrationId" = N'20260604135925_InitialCreate';

COMMIT;
