# Plan de implementación: cierre de brechas de persistencia (Unidad 3)

## Contexto

La auditoría [`AUDITORIA_UNIDAD_3.md`](../../AUDITORIA_UNIDAD_3.md) detectó tres requisitos
parcialmente cumplidos:

| # | Requisito | Brecha |
|---|-----------|--------|
| 1 | EF Core + SQLite y `DbContextFactory` | Sólo existe `IDesignTimeDbContextFactory`; el runtime usa un `DbContext` scoped de larga vida. |
| 3 | Migraciones versionadas + seed | No hay `HasData` ni inicializador de datos. |
| 4 | CRUD + paginación + búsqueda | CRUD y paginación completos, pero no hay búsqueda textual. |

## Decisiones

- Se usa `AddDbContextFactory<TareaDbContext>` y contextos de corta vida por operación.
- El seed se implementa con un **inicializador idempotente** después de `Migrate()`, no con
  `HasData`, para no romper bases existentes que ya tengan categorías migradas.
- La búsqueda textual cubre sólo `Tarea` (título y descripción); el listado de categorías
  sigue sin paginación ni búsqueda.
- No se agregan migraciones: las seis existentes siguen siendo el versionado del esquema.

## 1. `DbContextFactory` en runtime

### Infraestructura

Archivo: `src/Tareas.Infrastructure/DependencyInjection.cs`

- Reemplazar `services.AddDbContext<TareaDbContext>(...)` por
  `services.AddDbContextFactory<TareaDbContext>(options => options.UseSqlite(cadena))`.
- `AddDbContextFactory` también registra el contexto como scoped, por lo que
  `Program.cs` puede seguir resolviéndolo si lo necesita.

Archivo: `src/Tareas.Infrastructure/Repositories/TareaRepository.cs`

- Inyectar `IDbContextFactory<TareaDbContext>` en lugar de `TareaDbContext`.
- Crear un contexto por operación con `await using var contexto = await _factory.CreateDbContextAsync(ct);`.
- Aplicar en `ObtenerPaginadoAsync`, `ObtenerPorIdAsync`, `AgregarAsync`,
  `ActualizarAsync`, `DarDeBajaAsync`, `RestaurarAsync`, `CambiarCompletadaAsync` y
  `ObtenerEstadisticasAsync`.
- Eliminar el bloque defensivo de `Detached` en `ActualizarAsync`: con un contexto nuevo
  ya no puede existir conflicto de identidad.

Archivo: `src/Tareas.Infrastructure/Repositories/CategoryRepository.cs`

- Mismo cambio de inyección y creación de contexto por método.
- Eliminar el `Detached` defensivo de `ActualizarAsync`.

Archivo: `src/Tareas.Web/Program.cs`

- Resolver `IDbContextFactory<TareaDbContext>` en el scope de arranque y crear el contexto
  para `Database.Migrate()` y para el seed.

## 2. Seed idempotente

Archivo nuevo: `src/Tareas.Infrastructure/Persistence/SeedData.cs`

- Método `InicializarAsync(TareaDbContext contexto, CancellationToken ct = default)`.
- Categorías por defecto: `Trabajo`, `Personal`, `Estudio`, `Salud`, con descripciones
  válidas según `CategoryReglas`.
- Leer los nombres existentes, normalizarlos (`Trim().ToUpperInvariant()`) y crear sólo los
  faltantes con `Category.Crear`.
- Guardar únicamente si hay categorías nuevas.

Archivo: `src/Tareas.Web/Program.cs`

- Invocar `await SeedData.InicializarAsync(db);` inmediatamente después de `Migrate()`.

Tests: `tests/Tareas.Tests/Integration/SeedDataTests.cs`

- Ejecutar el inicializador dos veces y verificar que no duplica.
- Precargar una categoría con el mismo nombre en otra capitalización y verificar que no se
  duplica.

## 3. Búsqueda textual en tareas

Archivo: `src/Tareas.Application/Dtos/FiltroTareas.cs`

- Agregar `string? Texto = null` al final del record, conservando la compatibilidad
  posicional de las llamadas existentes.

Archivo: `src/Tareas.Infrastructure/Repositories/TareaRepository.cs`

- En `ExtensionesQueryableTarea.AplicarFiltros`, cuando `Texto` no esté vacío, aplicar:
  `EF.Functions.Like(t.Titulo, patron) || EF.Functions.Like(t.Descripcion, patron)` con
  `patron = $"%{texto.Trim()}%"`.

Archivo: `src/Tareas.Web/Components/Features/Tareas/TareaEstado.cs`

- Exponer `FiltroTextoTexto`.
- Agregar `SetFiltroTextoAsync(string? texto)`, normalizar el texto y reiniciar la página
  con `ReiniciarYCargarAsync()`.

Archivo: `src/Tareas.Web/Components/Shared/FilterBar.razor`

- Agregar parámetros `TextoSeleccionado` y `OnTextoChanged`.
- Agregar un `<input type="search">` con label accesible y debounce de ~300 ms mediante
  `CancellationTokenSource`; implementar `IDisposable` para cancelar el token.

Archivos: `src/Tareas.Web/Components/Pages/ListaTareas.razor` y `.razor.cs`

- Conectar el texto al `FilterBar` y delegar en `TareaEstado.SetFiltroTextoAsync`.

Tests:

- `tests/Tareas.Tests/Integration/TareaRepositoryFiltrosTests.cs`: búsqueda por título,
  por descripción, sin coincidencias, ignorando mayúsculas y combinada con otro filtro.
- `tests/Tareas.Tests/Application/Fakes.cs`: replicar el filtro de texto en
  `FakeTareaRepository`.

## 4. Adaptación de tests a la factory

Archivo nuevo: `tests/Tareas.Tests/Integration/TestDbContextFactory.cs`

- Implementar `IDbContextFactory<TareaDbContext>` sobre las mismas `DbContextOptions` y la
  conexión SQLite in-memory abierta.

Archivos a actualizar:

- `tests/Tareas.Tests/Integration/CategoryRepositoryTests.cs`
- `tests/Tareas.Tests/Integration/TareaRepositoryActualizarTests.cs`
- `tests/Tareas.Tests/Integration/TareaRepositoryBajaTests.cs`
- `tests/Tareas.Tests/Integration/TareaRepositoryFiltrosTests.cs`

Patrón: mantener `_contexto` para sembrar y verificar, y construir los repositorios con la
factory de test.

## 5. Documentación

- Actualizar `AUDITORIA_UNIDAD_3.md`: requisitos 1, 3 y 4 a **CUMPLE**.
- Actualizar `README.md`: mencionar el seed de categorías y la búsqueda textual.

## Verificación

```bash
dotnet build PW_Tareas.sln
dotnet test
dotnet ef migrations list --project src/Tareas.Infrastructure --context TareaDbContext
```

Comprobación funcional:

- La aplicación arranca, migra y crea las categorías por defecto una sola vez.
- Reiniciar la aplicación no duplica categorías.
- Buscar por título o descripción filtra server-side y vuelve a la página 1.
- Crear, editar, completar, dar de baja y restaurar tareas siguen funcionando sin
  conflictos de tracking.
