-- =====================================================
-- Seed: Registrar migracion como ya aplicada en produccion
-- Ejecutar UNA SOLA VEZ contra la BD existente (tareas.db)
-- usando SQLite Browser u otra herramienta SQL
-- =====================================================

CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260603014204_Initialcreate', '8.0.26');
