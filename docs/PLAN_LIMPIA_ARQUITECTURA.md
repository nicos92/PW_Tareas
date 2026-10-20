# Plan de Migración a Clean Architecture - PW_Tareas

## Contexto

Requisito original (`requerimiento_arquitectura.md`): revisar el código buscando inconsistencias
para luego pasar el proyecto a una arquitectura limpia, separando el front end del backend y
creando nuevas librerías de datos.

**Decisión tomada:** Clean Architecture por proyectos dentro de la misma solución, **sin API HTTP**.
La separación es por capas (librerías), no por frontera de red.

**Restricciones acordadas:**

- Target Framework: **`net8.0`** (no se migra a net10.0 en este refactor)
- Se **eliminan** los repositorios no registrados
- Se **agrega** proyecto de tests

**Precondición verificada:** `dotnet build PW_Tareas.sln` → **0 warnings, 0 errors**.
El árbol de trabajo está limpio. Las fases de este plan parten de ese estado.

---

## Diagnóstico: hallazgos

### Documentación desalineada

| Severidad | Hallazgo | Referencia |
|---|---|---|
| 🔴 | El README **miente**: afirma que `TareaProtectedLocalStorageRepository` es el activo, cuando está sin registrar | `README.md:59` vs `Program.cs:38` |

### Inconsistencias arquitectónicas

| Severidad | Hallazgo | Referencia |
|---|---|---|
| 🔴 | **Fuga de persistencia a la capa UI**: `GuardarArchivoAsync` duplicado byte a byte en dos páginas | `NuevaTarea.razor:56-71` == `EditarTarea.razor:94-109` |
| 🔴 | **`EliminarArchivo` triplicado** (lógica de `File.Delete` en la capa de presentación) | `EditarTarea.razor:111-120`, `TareaState.cs:105-114` |
| 🔴 | **Traversal de ruta**: la ruta proveniente de la BD se concatena con `WebRootPath` sin validar contención. `TrimStart('/')` no bloquea segmentos `..` | `TareaState.cs:109` |
| 🔴 | **`IWebHostEnvironment` inyectado en componentes Blazor** (dependencia del host web en la vista) | `NuevaTarea.razor:27`, `EditarTarea.razor:42` |
| 🔴 | **426 líneas de código muerto**: 3 de 4 repositorios sin registrar, cada uno copiando el mismo bloque de filtros/paginación/estadísticas | `Infrastructure/Repositories/*` |
| 🔴 | **Semánticas divergentes bajo la misma interfaz**: `Update` sobrescribe *todas* las columnas en SQLite, solo 2 campos en memoria | `TareaSqliteRepository.cs:40` vs `TareaRepository.cs:31-32` |
| 🔴 | **Strings mágicos como protocolo**: `"Completadas"` / `"Vencidas"` / `"VencenHoy"` / `"ATiempo"` viajan por la interfaz, se comparan con `switch` y se eligen desde `<select>`. Un typo no compila, sólo devuelve datos sin filtrar | `ITareaRepository.cs:70-74`, `TareaSqliteRepository.cs:96-109`, `FilterBar.razor:9-32` |
| 🔴 | **`TareaModel` mezcla 4 responsabilidades**: entidad EF (`[Key]`, `[DatabaseGenerated]`), anotaciones de validación **que también definen el esquema SQL** (`[StringLength]` → `maxLength: 50`), regla de negocio (`IValidatableObject`) y contrato de `EditForm` | `TareaModel.cs:8-46`, `Migrations/20260603014204_Initialcreate.cs:22` |
| 🔴 | **Cadena de conexión hardcodeada** con identidad de desarrollador (`"Nicolas_Sandoval"`) e I/O de directorios en el composition root. Sin `ConnectionStrings` en `appsettings.json` y sin `IDesignTimeDbContextFactory`, por lo que **`dotnet ef` no puede construir el contexto** | `Program.cs:17-33` |
| 🟠 | **`GetEstadisticasAsync` carga la tabla entera** y hace 8 `Count()` en memoria. Es la ruta caliente: se ejecuta en cada cambio de filtro | `TareaSqliteRepository.cs:128-141`, `TareaState.cs:45` |
| 🟠 | **No hay capa de aplicación**: `TareaState` es simultáneamente state container, fachada de datos, hub de eventos y servicio de archivos | `Shared/TareaState.cs:6` (141 líneas) |
| 🟠 | **Bug latente en `Toggle`**: resuelve la tarea desde `ResultadoPaginado.Items` (la página cargada), no desde el repositorio. Un toggle fuera de la página actual no hace nada en silencio | `TareaState.cs:118` |
| 🟠 | **7 suscriptores manuales** al `event Action OnChange`, con `-=` manual y riesgo de fuga | `Home.razor:41,47`, `ListaTareas.razor:104,178`, `StatsPanel.razor:32,38` |
| 🟠 | **Fragmentación de render modes**: `MainLayout` SSR estático hospeda `NavMenu` y `StatsPanel` interactivos fuera del `@Body` → 2-3 circuits SignalR por página | `MainLayout.razor:16` |
| 🟠 | **Páginas ruteables en la carpeta de componentes** | `Components/Shared/ListaTareas.razor`, `Components/Shared/Tarea.razor` |
| 🟠 | **Migraciones aplicadas sin `try/catch`** en cada arranque | `Program.cs:61-65` |
| 🟠 | **Inserción manual de `__EFMigrationsHistory`** fuera de EF Core | `docs/seed-migracion-produccion.sql` |
| 🟠 | **Cero proyectos de tests** | solución con un solo proyecto |
| 🟢 | `@using` muertos que referencian la capa de datos | `Tarea.razor:2-3`, `_Imports.razor:1-2` |
| 🟢 | `PesoArchivoAttribute.IsValid` siempre retorna `Success` (stub sin usos) | `Validation/ArchivoValidationAttribute.cs:27-30` |
| 🟢 | `Migrate()` síncrono y sin logging en el arranque | `Program.cs:61-65` |
| 🟢 | Namespace mal escrito de forma consistente: `Infraestructure` | todo `Infrastructure/` |
| 🟢 | Sin `CancellationTokens` en ningún método de la capa de datos | `ITareaRepository.cs` |
| 🟢 | `docs/PLAN_DESARROLLO.md` ya lista varios de estos bugs como deuda conocida | `PLAN_DESARROLLO.md:355-469` |

---

## Arquitectura objetivo

```
PW_Tareas.sln
├── src/
│   ├── Tareas.Domain/         net8.0 · SIN dependencias
│   ├── Tareas.Application/    net8.0 → sólo Domain
│   ├── Tareas.Infrastructure/ net8.0 → Application (EF Core 8.0.26)
│   └── Tareas.Web/            net8.0 → Application + Infrastructure  ← FRONT END
└── tests/
    └── Tareas.Tests/          xUnit + FluentAssertions
```

**Regla de dependencia:** `Web → Application → Domain`, y `Infrastructure → Application`.
**Domain no depende de nadie.** La UI nunca ve entidades de dominio, `DbContext` ni strings de
filtro — sólo DTOs.

El nombre del namespace se corrige de `Infraestructure` a `Tareas.Infrastructure`.

`docfx.json` usa el glob recursivo `**/*.csproj`, por lo que mover el proyecto
**no** rompe la documentación existente.

---

## Fase 1 - Estructura de solución

1. Crear los 4 proyectos en `src/` vía `dotnet new classlib` / `dotnet new web`.
2. Crear `tests/Tareas.Tests` vía `dotnet new xunit`.
3. Agregar todos a `PW_Tareas.sln`.
4. **Mover** `TareasBlazor.csproj` → `src/Tareas.Web/Tareas.Web.csproj`
   (`Microsoft.NET.Sdk.Web`).
5. Agregar referencias de proyecto:
   - `Tareas.Domain` → ninguna
   - `Tareas.Application` → `Tareas.Domain`
   - `Tareas.Infrastructure` → `Tareas.Application`
   - `Tareas.Web` → `Tareas.Application`, `Tareas.Infrastructure`
   - `Tareas.Tests` → `Tareas.Domain`, `Tareas.Application`, `Tareas.Infrastructure`
6. Mover `wwwroot/`, `Components/`, `Program.cs`, `appsettings*.json`, `Properties/` a
   `src/Tareas.Web/`.

**Criterio de salida:** `dotnet build` verde con proyectos vacíos y la app arrancable.

---

## Fase 2 - Domain (sin dependencias)

Estructura:

```
src/Tareas.Domain/
├── Entidades/Tarea.cs
├── ValueObjects/Prioridad.cs
├── ValueObjects/Categoria.cs
├── Reglas/TareaReglas.cs
└── Errores/ViolacionRegla.cs
```

- `Tarea.cs` — entidad pura, **cero anotaciones**. Se elimina `IValidatableObject`.
- `Prioridad.cs` / `Categoria.cs` — extraídos de `TareaModel.cs:49-64` a archivos propios.
- `TareaReglas.cs` — invariantes extraídos de `TareaModel.cs:40-46` y de
  `Validation/FutureDateAttribute.cs`:
  - fecha de vencimiento debe ser futura;
  - las tareas de prioridad Alta deben vencer en ≤ 7 días.
  - `Tarea.Crear(...)` es la única vía de construcción y valida todo.
- `ViolacionRegla.cs` — record con `Campo` + `Mensaje`. **Los mensajes en español viven acá**
  como fuente única; la UI los muestra directamente. Es un compromiso pragmático consciente:
  en una aplicación pequeña el texto de usuario es texto de la aplicación, no del dominio.

**Decisión de diseño: `IdPublic` pasa de `string` a `Guid`.**

El mapeo usa `HasConversion<string>()`, por lo que la columna sigue siendo `TEXT` en el mismo
formato de 36 caracteres con guiones que genera hoy `NuevaTarea.razor:45`. **Los datos existentes
y el seed se conservan sin cambios.**

**Criterio de salida:** `Tarea.Crear` lanza `ViolacionRegla` con las reglas de negocio; proyecto
de Domain sin ningún `using` de EF Core ni de DataAnnotations.

---

## Fase 3 - Application

Estructura:

```
src/Tareas.Application/
├── Abstractions/ITareaRepository.cs
├── Abstractions/IArchivoStorage.cs
├── Abstractions/ISystemClock.cs
├── CasosDeUso/
│   ├── Consultas/ObtenerTareasQuery.cs
│   ├── Consultas/ObtenerTareaPorIdQuery.cs
│   ├── Consultas/ObtenerEstadisticasQuery.cs
│   ├── Comandos/CrearTareaCommand.cs
│   ├── Comandos/ActualizarTareaCommand.cs
│   ├── Comandos/EliminarTareaCommand.cs
│   └── Comandos/CambiarCompletadaTareaCommand.cs
├── Dtos/
│   ├── TareaDto.cs
│   ├── EstadisticasTareas.cs
│   ├── PaginadoResult.cs
│   ├── ParametrosPaginacion.cs
│   └── FiltroTareas.cs
├── Filtros/EstadoFiltro.cs
├── Filtros/VencimientoFiltro.cs
├── Validation/ (FutureDateAttribute.cs, ArchivoImagenAttribute.cs)
└── DependencyInjection.cs
```

### Puertos (abstracciones que define esta capa)

- **`ITareaRepository`** — reescrito. Recibe `FiltroTareas` tipado en lugar de `string?`,
  devuelve entidades de dominio, y **acepta `CancellationToken`** en todos los métodos:

  ```csharp
  Task<PaginadoResult<Tarea>> ObtenerPaginadoAsync(
      ParametrosPaginacion paginacion, FiltroTareas filtro, CancellationToken ct = default);
  Task<Tarea?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
  Task AgregarAsync(Tarea tarea, CancellationToken ct = default);
  Task ActualizarAsync(Tarea tarea, CancellationToken ct = default);
  Task EliminarAsync(Guid id, CancellationToken ct = default);
  Task CambiarCompletadaAsync(Guid id, bool completada, CancellationToken ct = default);
  Task<EstadisticasTareas> ObtenerEstadisticasAsync(CancellationToken ct = default);
  Task EliminarTodasAsync(CancellationToken ct = default);
  ```

- **`IArchivoStorage`** — absorbe la I/O de archivos duplicada:

  ```csharp
  Task<string> GuardarAsync(Stream contenido, string extension, CancellationToken ct = default);
  Task EliminarAsync(string rutaRelativa, CancellationToken ct = default);
  ```

- **`ISystemClock`** — hace testeable el `DateTime.Now` de la regla de 7 días y del filtro
  "VencenHoy".

### Filtros tipados (reemplazan los strings mágicos)

```csharp
public enum EstadoFiltro       { Todas, Completadas, Pendientes }
public enum VencimientoFiltro  { Todos, Vencidas, VencenHoy, ATiempo }
public sealed record FiltroTareas(Prioridad? Prioridad, EstadoFiltro Estado,
                                  VencimientoFiltro Vencimiento);
```

### Casos de uso

Clases simples, **sin MediatR ni paquetes nuevos**:

| Caso de uso | Responsabilidad | Absorbe de |
|---|---|---|
| `ObtenerTareasQuery` | Paginar + filtrar + estadísticas en paralelo | `TareaState.cs:38-54` |
| `ObtenerTareaPorIdQuery` | Lookup por `Guid` | `TareaState.cs:125-128` |
| `ObtenerEstadisticasQuery` | Agregados | `TareaState.cs:45` |
| `CrearTareaCommand` | Validar vía dominio, persistir, generar `IdPublic` | `NuevaTarea.razor:45-52` |
| `ActualizarTareaCommand` | Validar y actualizar | `TareaState.cs:130-134` |
| `EliminarTareaCommand` | Eliminar archivo huérfano, luego la tarea, luego reajustar página | `TareaState.cs:89-103` |
| `CambiarCompletadaTareaCommand` | Toggle | `TareaState.cs:116-123` |

`EliminarTareaCommand` es el que absorbe la orquestación de borrar el archivo huérfano
hoy repartida entre `TareaState.cs:92-95` y `TareaState.cs:105-114`.

### DTOs

- `TareaDto` — contrato de la UI. **Las `DataAnnotations` se mueven acá**, no en la entidad:
  así el esquema SQL deja de depender de la validación del formulario.
- `EstadisticasTareas`, `PaginadoResult<T>`, `ParametrosPaginacion` — movidos desde `Models/`.
- `Validation/FutureDateAttribute.cs` y `ArchivoImagenAttribute.cs` se mueven a Application
  (son contrato de formulario, no regla de dominio). **Se borra el stub muerto
  `PesoArchivoAttribute`.**

**Criterio de salida:** `dotnet build` verde y ningún archivo de Application con
`using Microsoft.EntityFrameworkCore`.

---

## Fase 4 - Infrastructure

Estructura:

```
src/Tareas.Infrastructure/
├── Persistence/TareaDbContext.cs
├── Persistence/Configuraciones/TareaEntityTypeConfiguration.cs
├── Persistence/Migrations/
├── Repositories/TareaRepository.cs
├── Almacenamiento/ArchivoStorage.cs
├── Time/SystemClock.cs
├── DependencyInjection.cs
└── AppDbContextDesignTimeFactory.cs
```

### Eliminación de código muerto

| Archivo | Líneas | Motivo |
|---|---|---|
| `Infrastructure/Repositories/TareaRepository.cs` | 127 | Sin registrar |
| `Infrastructure/Database/InMemory.cs` | 9 | Estado estático mutable global |
| `Infrastructure/Repositories/TareaLocalStorageRepository.cs` | 143 | Sin registrar |
| `Infrastructure/Repositories/TareaProtectedLocalStorageRepository.cs` | 156 | Sin registrar; su `ProtectedLocalStorage` tampoco lo está |
| `wwwroot/js/tareas.js` | 14 | Sólo lo usaba `TareaLocalStorageRepository` |
| `docs/seed-migracion-produccion.sql` | 13 | Inserción manual de `__EFMigrationsHistory` |

### Migración EF Core

- Configuración **100% Fluent** en `TareaEntityTypeConfiguration.cs`:
  - `HasConversion<string>()` para `Prioridad`, `Categoria` e `IdPublic`
  - conversor `DateOnly?` ↔ `DateTime?` para `FechaVencimiento`
  - índice único en `IdPublic`
  - longitudes máximas explícitas (`50`, `200`) — mismos valores que hoy, por lo que el
    esquema **no cambia** por este punto
- **Una migración nueva** que:
  - aplica `IdPublic` como string↔Guid (sin cambio de tipo de columna real) y
    `Categoria` INTEGER→TEXT (**sí** altera la columna: exige regenerar
    `docs/seed-tareas.sql`)

> Se decide alinear `Categoria` con `Prioridad` (ambos como TEXT) para eliminar la
> asimetría latente. Es el único cambio de esquema real de todo el refactor.

### Repositorio EF

- Una **sola** copia de la lógica de filtros, extraída a una especificación parametrizable:

  ```csharp
  internal static IQueryable<Tarea> AplicarFiltros(
      this IQueryable<Tarea> query, FiltroTareas filtro, DateOnly hoy)
  ```

- `ObtenerEstadisticasAsync` pasa a agregación real en SQL (`CountAsync` con predicados),
  **sin** `ToListAsync()` de la tabla completa. Es la ruta caliente.

### Almacenamiento de archivos

`ArchivoStorage.cs` centraliza el `FileStream` que hoy vive en dos componentes:

- **Validación de contención de ruta** (`Path.GetFullPath` + `StartsWith(root)`) — cierra
  el traversal de `TareaState.cs:109`
- Límite de 1 MB como constante configurable, en lugar del literal `1024 * 1024` repetido
  en 4 lugares
- Genera el nombre de archivo aleatorio; **no** usa la extensión del nombre del cliente

### Configuración

- `appsettings.json`:

  ```json
  {
    "ConnectionStrings": { "Tareas": "" },
    "Storage": { "UploadsPath": "uploads", "MaxFileSizeBytes": 1048576 }
  }
  ```

- Se elimina el path `"Nicolas_Sandoval"` de `Program.cs:21`.
- Se agrega `IDesignTimeDbContextFactory<TareaDbContext>` para que `dotnet ef migrations`
  funcione sin la ejecución de la aplicación.
- `Migrate()` pasa a un paso guardado con `try/catch` y logging.

**Criterio de salida:** `dotnet ef database update` aplica la migración sobre la base real
sin perder datos; estadísticas en SQL sin carga de tabla completa.

---

## Fase 5 - Front end

### Composition root

`Program.cs` baja de 67 a ~20 líneas:

```csharp
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<TareaEstado>();
```

### Separación de responsabilidades en la capa UI

- `Shared/TareaState.cs` → **`Components/Features/Tareas/TareaEstado.cs`**, dejando de ser
  god-object: conserva **sólo estado de UI** (filtros, página, `IsLoading`) y delega a los
  casos de uso. **Pierde `IWebHostEnvironment` y toda la I/O de archivos.**
- **Se corrige el bug de `Toggle`**: se resuelve la tarea por id en el repositorio, no desde
  la página cargada (`TareaState.cs:118`).
- Los 7 sitios `+= OnChange` / `-= OnChange` pasan a un helper único con `IDisposable`,
  eliminando el riesgo de fuga.
- `NuevaTarea.razor` / `EditarTarea.razor` dejan de inyectar `IWebHostEnvironment` y de
  hacer `FileStream`; entregan el `IBrowserFile` al caso de uso correspondiente.

### Reorganización de carpetas

- `ListaTareas.razor` y `Tarea.razor` se mueven de `Components/Shared/` a
  `Components/Pages/` (hoy son páginas ruteables mezcladas con componentes reutilizables).
- Se extraen code-behinds `.razor.cs` de los 4 componentes más grandes:

  | Componente | Líneas |
  |---|---|
  | `ListaTareas.razor` | 181 |
  | `Breadcrum.razor` | 131 |
  | `EditarTarea.razor` | 121 |
  | `ImageUploader.razor` | 113 |

- `Helpers/PrioridadHelper.cs` se mueve al proyecto Web (hoy vive en un `Helpers/` de raíz
  importado globalmente por `_Imports.razor:15`).

### Limpieza

- `@using` muertos: `Tarea.razor:2-3` (referencia `Infrastructure.*`), `_Imports.razor:1-2`
  (`System.Net.Http*`, restos de la plantilla WASM).
- `OnImagenChanged` vacío en `NuevaTarea.razor:34-36`.
- `_procesando` de `ConfirmDialog.razor:40` nunca se resetea tras una excepción.
- `NavScroll.razor.css` (0 bytes).

### Render modes

`MainLayout` es SSR estático pero hospeda `NavMenu` y `StatsPanel` **interactivos y fuera**
del `@Body` (`MainLayout.razor:16`) → **2-3 circuits SignalR por página**.
Se unifica poniendo `@rendermode InteractiveServer` en el layout y quitándolo de los hijos.

**Criterio de salida:** ninguna página inyecta `IWebHostEnvironment`; ninguna realización de
`File.*` en `Components/`; un solo circuito SignalR por página.

---

## Fase 6 - Tests

`tests/Tareas.Tests` con **xUnit + FluentAssertions**, usando `FakeClock`,
`FakeArchivoStorage` y un `ITareaRepository` en memoria como doubles.

### Tests de dominio

- `Tarea.Crear` con datos válidos.
- Regla de 7 días para prioridad Alta: rechaza fecha > 7 días, acepta ≤ 7 días.
- Fecha de vencimiento no futura.
- Límites de longitud de título (3-50) y descripción (200).

### Tests de aplicación

- Cada caso de uso contra los puertos fake.
- `EliminarTareaCommand`: elimina el archivo huérfano y **no** falla si el archivo ya no existe.
- Filtros: se pasan correctamente al repositorio (regresión de los strings mágicos).
- `CambiarCompletadaTareaCommand`: resuelve la tarea por id aunque no esté en la página actual
  (regresión del bug de `Toggle`).

### Tests de integración

- `TareaDbContext` sobre SQLite en memoria: verifica que la especificación de filtros
  **se traduce a SQL correctamente**. Es el lugar exacto donde viven los bugs actuales.

---

## Fase 7 - Documentación

- `README.md`: corregir la tabla de repositorios (afirma que `protectedLocalStorage` es el
  activo — es falso) y actualizar el árbol de carpetas.
- Eliminar `docs/seed-migracion-produccion.sql`.
- Regenerar `docs/seed-tareas.sql` con el esquema nuevo (`Categoria` como TEXT) o convertirlo
  en un seeder idempotente de EF Core.
- `docs/PLAN_DESARROLLO.md`: marcar los bugs de FASE 2 que este refactor resuelve —
  doble suscripción (`:355`), `PesoArchivoAttribute` (`:382`), archivos huérfanos (`:419`),
  `DbContext` registrado sin usar (`:469`).
- Reemplazar el contenido de `requerimiento_arquitectura.md` con la arquitectura final.

---

## Verificación

Cada fase termina con:

```powershell
dotnet build PW_Tareas.sln     # 0 errores, 0 warnings
dotnet test                    # verde
dotnet ef migrations list --project src/Tareas.Infrastructure
```

Comprobación manual al finalizar: arrancar la aplicación y recorrer alta, edición, borrado,
toggle, filtros, paginación y subida de imagen, verificando que `Migrate()` aplique la
migración nueva sobre la base real sin perder datos.

---

## Orden de riesgo

| Riesgo | Fase | Mitigación |
|---|---|---|
| Base de datos existente | 4 | Conversores preservan el tipo de columna; el seed se regenera |
| Pérdida de datos en `ActualizarTarea` | 3 | El `Update` ciego actual pasa a copia explícita de campos (ya es la semántica del repo in-memory) |
| Regresión funcional en la UI | 5 | Los tests de Application cubren los casos de uso antes de tocar componentes |
| Cambio de esquema (`Categoria`) | 4 | Único cambio real; regenera el seed y exige migración |

---

## Resumen de impacto

| Métrica | Antes | Después |
|---|---|---|
| Proyectos en la solución | 1 | 6 |
| Implementaciones de `ITareaRepository` | 4 (1 activa, 3 muertas) | 1 |
| Copias de la lógica de filtros | 4 | 1 |
| Copias de `GuardarArchivoAsync` | 2 | 1 (`IArchivoStorage`) |
| Copias de `EliminarArchivo` | 3 | 1 (`IArchivoStorage`) |
| Archivos en `Models/` | 4 | 0 (repartidos en Domain + Application) |
| Capa de aplicación | 1 god-object (141 líneas) | 7 casos de uso + DTOs |
| Proyectos de test | 0 | 1 |
| Líneas de código muerto | ~440 | 0 |
| Errores de build | 0 | 0 |
