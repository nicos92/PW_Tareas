# Auditoría de Código — UNIDAD 3: Persistencia

**Proyecto:** `PW_Tareas` · **Acceso a datos:** EF Core 8 + SQLite · **Capa de persistencia:** `src/Tareas.Infrastructure` · **Consumidores:** `src/Tareas.Application` (casos de uso) y `src/Tareas.Web` (UI Blazor Server).

---

## Estado de Cumplimiento - UNIDAD 3

| # | Requisito | Estado |
|---|-----------|--------|
| 1 | EF Core + SQLite/SQL Server y `DbContextFactory` | **NO CUMPLE (parcial)** — SQLite sí; falta el factory en runtime |
| 2 | ≥ 2 entidades relacionadas con navegación | **CUMPLE** |
| 3 | Migraciones versionadas + datos de seed | **NO CUMPLE (parcial)** — migraciones sí; seed ausente |
| 4 | CRUD + paginación + búsqueda | **NO CUMPLE (parcial)** — CRUD y paginación sí; búsqueda textual ausente |

---

### 1. EF Core con SQLite / SQL Server y DbContextFactory
- **Estado: NO CUMPLE (parcial)**
- **Evidencia / Ausencia:**
  - **Configuración en `Program.cs`:**
    - `Program.cs` (Líneas 21-22) — `builder.Services.AddApplication();` y `builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.WebRootPath);`.
    - `src/Tareas.Infrastructure/DependencyInjection.cs` (Líneas 26-30) — resolución de la cadena y **`services.AddDbContext<TareaDbContext>(options => options.UseSqlite(cadena));`** (SQLite confirmado).
    - `src/Tareas.Infrastructure/Persistence/RutaBaseDatos.cs` (Líneas 20-39) — resolución de la ruta del archivo `tareas.db` (config → `DATA_DIR` → carpeta local) con fallback a directorio temporal.
    - `Program.cs` (Líneas 58-64) — creación de scope y `db.Database.Migrate()` al iniciar.
  - **Uso de `DbContextFactory`:**
    - `src/Tareas.Infrastructure/TareaDbContextDesignTimeFactory.cs` (Líneas 13-33) — implementa **`IDesignTimeDbContextFactory<TareaDbContext>`**, pero es exclusivamente para `dotnet ef` (diseño), **no** para el runtime de la aplicación.
    - `src/Tareas.Infrastructure/Repositories/TareaRepository.cs` (Líneas 19-25) y `CategoryRepository.cs` (Líneas 14-19) — reciben **`TareaDbContext` scoped** por constructor.
    - `src/Tareas.Infrastructure/Persistence/TareaDbContext.cs` (Líneas 11-18) — contexto con `DbContextOptions` y constructor público.
  - **Qué falta:** no existe registro `AddDbContextFactory<TareaDbContext>(...)` ni inyección de `IDbContextFactory<TareaDbContext>` en ningún punto. Búsqueda global sin resultados (`grep -rn "AddDbContextFactory\|IDbContextFactory" src` → 0 coincidencias). En Blazor Server el `DbContext` scoped vive todo el circuito, lo que provoca contextos de larga vida, tracking acumulado y posibles conflictos de identidad (el propio código lo admite y parchea en `TareaRepository.cs:71-84` y `CategoryRepository.cs:63-70`).
  - **Recomendación técnica:**
    1. En `DependencyInjection.cs` reemplazar/añadir `services.AddDbContextFactory<TareaDbContext>(options => options.UseSqlite(cadena));` (y mantener `AddDbContext` sólo si algún consumidor lo requiere).
    2. Inyectar `IDbContextFactory<TareaDbContext>` en los repositorios y crear un contexto por operación: `await using var contexto = await _factory.CreateDbContextAsync(ct);`.
    3. Con ello se elimina el patrón de `Detached`/`EntityState.Modified` defensivo y se garantiza una instanciación segura por unidad de trabajo.

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
- **Estado: NO CUMPLE (parcial)** — hay migraciones versionadas, pero no hay seed.
- **Evidencia / Ausencia:**
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
    - **No existe seed.** Búsqueda global de `HasData`/`Seed`/`InsertData` en `src` → 0 coincidencias.
    - `TareaDbContext.cs` (Líneas 20-23) sólo aplica configuraciones; no hay `HasData`.
    - Los `migrationBuilder.Sql(...)` existentes **no son seed**: `CategoriaTexto.cs` (Líneas 16-24) convierte valores de enum ya existentes y `AgregaCategorias.cs` (Líneas 41-60) crea categorías **a partir de datos preexistentes** (`INSERT ... SELECT FROM Tareas`) y las vincula; en una base nueva esas sentencias no insertan ninguna fila.
  - **Recomendación técnica:**
    1. **Opción A (recomendada):** seed declarativo con `HasData` en `CategoryEntityTypeConfiguration`/`TareaEntityTypeConfiguration` usando IDs determinísticos:
       ```csharp
       entity.HasData(
           new { Id = 1, Name = "Backend", Description = "Tareas de backend" },
           new { Id = 2, Name = "Frontend", Description = "Tareas de frontend" });
       ```
       Generar la migración correspondiente con `dotnet ef migrations add SeedCategorias`.
    2. **Opción B:** inicializador personalizado (`IDbInitializer`/`SeedData.InicializarAsync`) invocado en `Program.cs` después de `Migrate()`, dentro del scope ya existente (Líneas 58-84).
    3. Mantener el seed idempotente (comprobar existencia antes de insertar) y usar datos estables para no romper migraciones futuras.

---

### 4. CRUD completo con paginación y búsqueda
- **Estado: NO CUMPLE (parcial)** — CRUD y paginación completos; **falta búsqueda textual**.
- **Evidencia / Ausencia:**
  - **Servicio o Repositorio CRUD:**
    - **Tareas** — contrato `src/Tareas.Application/Abstractions/ITareaRepository.cs` (Líneas 12-29): `ObtenerPaginadoAsync`, `ObtenerPorIdAsync`, `AgregarAsync`, `ActualizarAsync`, `DarDeBajaAsync`, `RestaurarAsync`, `CambiarCompletadaAsync`.
      - Implementación: `src/Tareas.Infrastructure/Repositories/TareaRepository.cs` — **Create** `AgregarAsync` (Líneas 63-67), **Read** `ObtenerPaginadoAsync` (Líneas 28-53) y `ObtenerPorIdAsync` (Líneas 55-61), **Update** `ActualizarAsync` (Líneas 69-86), **Delete** lógico `DarDeBajaAsync` (Líneas 88-98) + `RestaurarAsync` (Líneas 100-113) + `CambiarCompletadaAsync` (Líneas 115-125).
      - Casos de uso: `CrearTareaCommand.cs:17-38`, `ActualizarTareaCommand.cs:19-49`, `DarDeBajaTareaCommand.cs:11-12`, `RestaurarTareaCommand.cs:7-8`.
    - **Categorías** — contrato `ICategoryRepository.cs` (Líneas 11-25) e implementación `CategoryRepository.cs`: **Create** (Líneas 55-59), **Read** (Líneas 21-34), **Update** (Líneas 61-71), **Delete** (Líneas 73-83). Casos de uso `CreateCategoryCommand.cs:13-22`, `UpdateCategoryCommand.cs:12-24`, `DeleteCategoryCommand.cs:11-19`.
  - **Interfaz de Paginación y Búsqueda:**
    - Paginación: `ParametrosPaginacion.cs` (Líneas 3-15, con tope `MaxPageSize = 50`), `PaginadoResult.cs` (Líneas 3-20, `TotalPages`/`HasPrevious`/`HasNext`), `ObtenerTareasQuery.cs` (Líneas 11-24), repositorio con `CountAsync` + `Skip` + `Take` (`TareaRepository.cs:44-52`).
    - UI: `Shared/Pagination.razor` (Líneas 1-54) y su uso en `Pages/ListaTareas.razor` (Líneas 49-56); estado en `Features/Tareas/TareaEstado.cs` (Líneas 147-157) y `ParametrosPaginacion PaginacionActual` (Línea 50).
    - **Búsqueda:** **no implementada.** `FiltroTareas.cs` (Líneas 10-14) sólo contiene `Prioridad`, `Estado`, `Vencimiento` y `EstadoBaja`; `ExtensionesQueryableTarea.AplicarFiltros` (`TareaRepository.cs:166-199`) aplica únicamente esos enums, sin `Contains`/`EF.Functions.Like` sobre `Titulo` o `Descripcion`; la UI `Shared/FilterBar.razor` (Líneas 5-29) sólo renderiza cuatro `<SelectFilter>`, sin campo de texto; no existe `InputText` de búsqueda en ninguna página. Los filtros existentes son de selección, no "búsqueda" (texto libre).
  - **Recomendación técnica:**
    1. Extender `FiltroTareas` con `string? Texto = null` (o `Busqueda`).
    2. En `ExtensionesQueryableTarea.AplicarFiltros` añadir:
       ```csharp
       if (!string.IsNullOrWhiteSpace(filtro.Texto))
       {
           var patron = $"%{filtro.Texto.Trim()}%";
           query = query.Where(t => EF.Functions.Like(t.Titulo, patron)
                                 || EF.Functions.Like(t.Descripcion, patron));
       }
       ```
    3. Añadir `SetFiltroTextoAsync` en `TareaEstado` que reinicie `PaginacionActual.Page = 1` y recargue (mismo patrón que `SetFiltroPrioridadAsync`, Líneas 105-113).
    4. En `FilterBar.razor`, agregar un `<input type="search" @oninput=...>` (con debounce) y un `EventCallback<string> OnTextoChanged`; conectarlo en `ListaTareas.razor` (Líneas 13-17).
    5. Opcional: replicar paginación + búsqueda en el listado de categorías (`CategoryRepository.ObtenerTodasAsync`, Líneas 21-27) si el alcance incluye esa entidad; hoy devuelve todo sin paginar.

---

## Conclusión

- **Requisito 1 — Parcial.** EF Core 8 + SQLite correctamente configurados y migraciones aplicadas al inicio, pero **no se usa `DbContextFactory` en runtime**; sólo existe un `IDesignTimeDbContextFactory` para `dotnet ef`. En Blazor Server esto es una deuda técnica relevante (DbContext scoped de larga vida).
- **Requisito 2 — Cumple.** `Tarea` ↔ `Category` con FK, navegación y configuración Fluent API completa (incluye índice, borrado restrictivo y filtro global).
- **Requisito 3 — Parcial.** Hay 6 migraciones versionadas con snapshot y aplicación automática, pero **no hay datos de seed** (`HasData` ni inicializador); los `Sql(...)` son migraciones de datos derivadas, no población inicial.
- **Requisito 4 — Parcial.** CRUD completo (con baja lógica y restauración) y paginación server-side con UI funcionando; **falta la búsqueda por texto** en DTO de filtro, repositorio y UI.
