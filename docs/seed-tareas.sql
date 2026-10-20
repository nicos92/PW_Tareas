-- =============================================================================
-- Seed idempotente de datos de ejemplo para PW_Tareas (TareasBlazor)
-- =============================================================================
-- Esquema de destino: tabla "Tareas" tras la migración 20261020093434_CategoriaTexto.
--
--   Id               INTEGER  PK AUTOINCREMENT
--   IdPublic         TEXT     UNIQUE (Guid en formato "D")
--   Titulo           TEXT     (máx. 50)
--   Descripcion      TEXT     (máx. 200)
--   Completada       INTEGER  0/1
--   FechaVencimiento TEXT     DateTime ISO ("yyyy-MM-dd HH:mm:ss")
--   Prioridad        TEXT     'Baja' | 'Media' | 'Alta'
--   Imagen           TEXT     (ruta relativa o cadena vacía)
--   Categoria        TEXT     'Ninguna' | 'Frontend' | 'Backend' | 'BaseDeDatos' | 'DevOps' | 'Testing'
--
-- Ejecutar con:
--     sqlite3 tareas.db < docs/seed-tareas.sql
--
-- El bloque es idempotente: sólo inserta las filas que aún no existen (por IdPublic),
-- por lo que puede ejecutarse varias veces sin duplicar datos.
-- =============================================================================

BEGIN TRANSACTION;

INSERT INTO Tareas (IdPublic, Titulo, Descripcion, Completada, FechaVencimiento, Prioridad, Imagen, Categoria)
SELECT '11111111-1111-1111-1111-111111111111',
       'Configurar pipeline CI',
       'Definir build, tests y publicación en el pipeline de integración continua.',
       0,
       '2099-01-15 00:00:00',
       'Alta',
       '',
       'DevOps'
WHERE NOT EXISTS (
    SELECT 1 FROM Tareas WHERE IdPublic = '11111111-1111-1111-1111-111111111111'
);

INSERT INTO Tareas (IdPublic, Titulo, Descripcion, Completada, FechaVencimiento, Prioridad, Imagen, Categoria)
SELECT '22222222-2222-2222-2222-222222222222',
       'Migrar a Clean Architecture',
       'Separar Domain, Application, Infrastructure y Web en proyectos independientes.',
       1,
       '2099-02-01 00:00:00',
       'Media',
       '',
       'Backend'
WHERE NOT EXISTS (
    SELECT 1 FROM Tareas WHERE IdPublic = '22222222-2222-2222-2222-222222222222'
);

INSERT INTO Tareas (IdPublic, Titulo, Descripcion, Completada, FechaVencimiento, Prioridad, Imagen, Categoria)
SELECT '33333333-3333-3333-3333-333333333333',
       'Revisar diseño del dashboard',
       'Ajustar tarjetas de estadísticas y colores según el sistema de diseño.',
       0,
       '2099-03-10 00:00:00',
       'Baja',
       '',
       'Frontend'
WHERE NOT EXISTS (
    SELECT 1 FROM Tareas WHERE IdPublic = '33333333-3333-3333-3333-333333333333'
);

INSERT INTO Tareas (IdPublic, Titulo, Descripcion, Completada, FechaVencimiento, Prioridad, Imagen, Categoria)
SELECT '44444444-4444-4444-4444-444444444444',
       'Escribir tests de integración',
       'Cubrir filtros y estadísticas del repositorio contra SQLite en memoria.',
       0,
       '2099-04-05 00:00:00',
       'Media',
       '',
       'Testing'
WHERE NOT EXISTS (
    SELECT 1 FROM Tareas WHERE IdPublic = '44444444-4444-4444-4444-444444444444'
);

COMMIT;
