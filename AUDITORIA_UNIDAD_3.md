# Auditoría de Código — UNIDAD 3: Persistencia

**Proyecto:** `PW_Tareas` · **Acceso a datos:** EF Core 8 + SQLite · **Capa de persistencia:** `src/Tareas.Infrastructure` · **Consumidores:** `src/Tareas.Application` (casos de uso) y `src/Tareas.Web` (UI Blazor Server).

---

## Estado de Cumplimiento - UNIDAD 3

| # | Requisito | Estado |
|---|-----------|--------|
| 1 | EF Core + SQLite/SQL Server y `DbContextFactory` | **CUMPLE** — SQLite + `AddDbContextFactory<TareaDbContext>` en runtime con contextos por operación |
| 2 | ≥ 2 entidades relacionadas con navegación | **CUMPLE** |
| 3 | Migraciones versionadas + datos de seed | **CUMPLE** — 6 migraciones + `SeedData` idempotente ejecutado al iniciar |
| 4 | CRUD + paginación + búsqueda | **CUMPLE** — CRUD, paginación y búsqueda textual por título/descripción |

---

### 1. EF Core con SQLite / SQL Server y DbContextFactory
- **Estado: CUMPLE**
- **Evidencia:**
  - **Configuración en `Program.cs`:**
    - `Program.cs` (Líneas 21-22) — `builder.Services.AddApplication();` y `builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.WebRootPath);`.
    - `src/Tareas.Infrastructure/DependencyInjection.cs` — resolución de la cadena y **`services.AddDbContextFactory<TareaDbContext>(options => options.UseSqlite(cadena));`** (SQLite confirmado).
    - `src/Tareas.Infrastructure/Persistence/RutaBaseDatos.cs` (Líneas 20-39) — resolución de la ruta del archivo `tareas.db` (config → `DATA_DIR` → carpeta local) con fallback a directorio temporal.
    - `Program.cs` — creación de scope, `IDbContextFactory<TareaDbContext>` y `await db.Database.MigrateAsync()` al iniciar.
  - **Uso de `DbContextFactory` en runtime:**
    - `src/Tareas.Infrastructure/Repositories/TareaRepository.cs` y `CategoryRepository.cs` — inyectan **`IDbContextFactory<TareaDbContext>`** y crean un contexto por operación con `await using var contexto = await _fabrica.CreateDbContextAsync(ct);`.
    - `src/Tareas.Infrastructure/TareaDbContextDesignTimeFactory.cs` (Líneas 13-33) — se conserva **`IDesignTimeDbContextFactory<TareaDbContext>`** exclusivamente para `dotnet ef`.
    - Se eliminó el patrón defensivo de `Detached`/`EntityState.Modified` que existía por el contexto scoped de larga vida.

---

### 2. Al menos 2 entidades con relación entre ellas
- **Estado: CUMPLE**
- **Evidencia / Ausencia:**
  - **Entidades y relación:**
    - `src/Tareas.Domain/Entidades/Category.cs` (Líneas 10-18) — entidad `Category` con `Id`, `Name`, `Description`.
    - `src/Tareas.Domain/Entidades/Tarea.cs` (Líneas 13-29) — entidad `Tarea`; **FK `CategoryId`** (Línea 27) y **navegación `Category?`** (Línea 28); asignación de la FK en `Crear` (Líneas 31-53) y `Actualizar` (Líneas 55-76).
  - **Configuración en `DbContext`:**
    - `src/Tareas.Infrastructure/Persistence/TareaDbContext.cs` (Líneas 13-14) — `DbSet<Tarea> Tareas` y `DbSet<Category> Categorias`; (Líneas 20-23) `ApplyConfigurationsFromAssembly`.
    - `src/Tareas.Infrastructure/Persistence/Configuraciones/TareaEntityTypeConfiguration.cs` (Líneas 41-46) — **Fluent API**: `HasOne(t => t.Category).WithMany().HasForeignKey(t => t.CategoryId).OnDelete(DeleteBehavior.Restrict)`, más índice en `CategoryId` (Línea 46) y filtro global de baja lógica (Línea 52).
    - `src/Tareas.Infrastructure/Persistence/Configuraciones/CategoryEntityTypeConfiguration.cs` (Líneas 17-31) — tabla `Categorias`, longitudes y **índice único** en `Name` (Líneas 30-31).
    - Relación materializada en consultas con `Include(t => t.Category)` (`TareaRepository.cs:33-35` y `:57-60`).
    - FK versionada en migraciones: `20261101125732_AgregaCategorias.cs` (Líneas 71-77) y `20261101144650_RestringeEliminacionCategoria.cs` (Líneas 13-24, `Restrict`).
  - **Nota:** la relación es 1→N (una categoría, muchas tareas) y la navegación inversa no está declarada (`WithMany()` sin colección en `Category`); es válido para el requisito, aunque podría añadirse `ICollection<Tarea> Tareas` en `Category` si se necesita navegación bidireccional.

---

### 3. Migraciones versionadas y datos de seed
- **Estado: CUMPLE** — migraciones versionadas + inicializador de seed idempotente.
- **Evidencia:**
  - **Carpeta de Migraciones:** `src/Tareas.Infrastructure/Persistence/Migrations/` (6 migraciones + snapshot):
    - `20260603014204_Initialcreate.cs` (+ `.Designer.cs`)
    - `20260604224301_AgregaCategoriaTarea.cs` (+ `.Designer.cs`)
    - `20261020093434_CategoriaTexto.cs` (+ `.Designer.cs`)
    - `20261031205118_AgregaBajaLogicaTarea.cs` (+ `.Designer.cs`)
    - `20261101125732_AgregaCategorias.cs` (+ `.Designer.cs`)
    - `20261101144650_RestringeEliminacionCategoria.cs` (+ `.Designer.cs`)
    - `TareaDbContextModelSnapshot.cs`
    - Versionado y aplicación: atributos `[Migration("...")]` en cada `.Designer.cs` (p. ej. `20261020093434_CategoriaTexto.Designer.cs:14-15`), snapshot del modelo en `TareaDbContextModelSnapshot.cs:13`, y aplicación automática en `Program.cs:63-64` (`db.Database.Migrate()`).
  - **Datos de Seed:**
    - `src/Tareas.Infrastructure/Persistence/SeedData.cs` — inicializador estático con las categorías por defecto `Trabajo`, `Personal`, `Estudio` y `Salud`.
    - `SeedData.InicializarAsync` consulta los nombres existentes, los normaliza y crea únicamente los faltantes; ejecutarlo dos veces no duplica ni pisa datos.
    - `Program.cs` invoca `await SeedData.InicializarAsync(db);` inmediatamente después de aplicar las migraciones.
    - Se eligió el inicializador idempotente (Opción B) en lugar de `HasData` para no chocar con categorías ya creadas por la migración `AgregaCategorias` en bases existentes.
    - Cobertura: `tests/Tareas.Tests/Integration/SeedDataTests.cs` verifica idempotencia y respeto de categorías preexistentes.

---

### 4. CRUD completo con paginación y búsqueda
- **Estado: CUMPLE** — CRUD, paginación y búsqueda textual completos.
- **Evidencia:**
  - **Servicio o Repositorio CRUD:**
    - **Tareas** — contrato `src/Tareas.Application/Abstractions/ITareaRepository.cs` (Líneas 12-29): `ObtenerPaginadoAsync`, `ObtenerPorIdAsync`, `AgregarAsync`, `ActualizarAsync`, `DarDeBajaAsync`, `RestaurarAsync`, `CambiarCompletadaAsync`.
      - Implementación: `src/Tareas.Infrastructure/Repositories/TareaRepository.cs` — **Create** `AgregarAsync` (Líneas 63-67), **Read** `ObtenerPaginadoAsync` (Líneas 28-53) y `ObtenerPorIdAsync` (Líneas 55-61), **Update** `ActualizarAsync` (Líneas 69-86), **Delete** lógico `DarDeBajaAsync` (Líneas 88-98) + `RestaurarAsync` (Líneas 100-113) + `CambiarCompletadaAsync` (Líneas 115-125).
      - Casos de uso: `CrearTareaCommand.cs:17-38`, `ActualizarTareaCommand.cs:19-49`, `DarDeBajaTareaCommand.cs:11-12`, `RestaurarTareaCommand.cs:7-8`.
    - **Categorías** — contrato `ICategoryRepository.cs` (Líneas 11-25) e implementación `CategoryRepository.cs`: **Create** (Líneas 55-59), **Read** (Líneas 21-34), **Update** (Líneas 61-71), **Delete** (Líneas 73-83). Casos de uso `CreateCategoryCommand.cs:13-22`, `UpdateCategoryCommand.cs:12-24`, `DeleteCategoryCommand.cs:11-19`.
  - **Interfaz de Paginación y Búsqueda:**
    - Paginación: `ParametrosPaginacion.cs` (Líneas 3-15, con tope `MaxPageSize = 50`), `PaginadoResult.cs` (Líneas 3-20, `TotalPages`/`HasPrevious`/`HasNext`), `ObtenerTareasQuery.cs` (Líneas 11-24), repositorio con `CountAsync` + `Skip` + `Take` (`TareaRepository.cs:44-52`).
    - UI: `Shared/Pagination.razor` (Líneas 1-54) y su uso en `Pages/ListaTareas.razor` (Líneas 49-56); estado en `Features/Tareas/TareaEstado.cs` (Líneas 147-157) y `ParametrosPaginacion PaginacionActual` (Línea 50).
    - **Búsqueda:** implementada de punta a punta.
      - `FiltroTareas.cs` incorpora `string? Texto = null`.
      - `ExtensionesQueryableTarea.AplicarFiltros` (`TareaRepository.cs`) aplica `EF.Functions.Like` sobre `Titulo` y `Descripcion` cuando `Texto` no está vacío.
      - `TareaEstado.SetFiltroTextoAsync` normaliza el texto y reinicia la paginación a la página 1 mediante `ReiniciarYCargarAsync()`.
      - `Shared/FilterBar.razor` renderiza un `<input type="search">` accesible con debounce de 300 ms y expone `EventCallback<string> OnTextoChanged`.
      - `Pages/ListaTareas.razor` conecta el campo de búsqueda con el estado.
      - Cobertura: `TareaRepositoryFiltrosTests` verifica búsqueda por título, por descripción, sin coincidencias, sin distinguir mayúsculas y combinada con otros filtros.

---

## Conclusión

- **Requisito 1 — Cumple.** EF Core 8 + SQLite con `AddDbContextFactory<TareaDbContext>` y contextos de corta vida por operación en ambos repositorios; se conserva el `IDesignTimeDbContextFactory` para `dotnet ef`.
- **Requisito 2 — Cumple.** `Tarea` ↔ `Category` con FK, navegación y configuración Fluent API completa (incluye índice, borrado restrictivo y filtro global).
- **Requisito 3 — Cumple.** Hay 6 migraciones versionadas con snapshot y aplicación automática, más un seed idempotente de categorías ejecutado al iniciar.
- **Requisito 4 — Cumple.** CRUD completo (con baja lógica y restauración), paginación server-side y búsqueda textual por título/descripción en DTO de filtro, repositorio y UI.
