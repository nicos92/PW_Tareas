# Desarrollo: Paginación con SQLite + Entity Framework Core

## Objetivo

Implementar paginación a nivel de base de datos para la lista de tareas, reemplazando el enfoque actual de carga masiva de datos a memoria y filtrado con LINQ.

## Contexto Técnico

### Flujo de datos ANTES de la paginación

```
SQLite (tareas.db)
  └─ TareaSqliteRepository.GetTareasAsync()
       └─ _context.Tareas.ToListAsync()  ← CARGA TODA LA TABLA
            └─ TareaState._tareas (List<TareaModel> en memoria)
                 └─ ListaTareas.TareasFiltradas (filtro LINQ en memoria)
                      └─ UI renderiza todo de una vez
```

### Problemas identificados

| Problema | Impacto |
|---|---|
| `GetTareasAsync()` carga toda la tabla con `ToListAsync()` | Alto consumo de memoria con muchos registros |
| Filtrado con LINQ en `TareasFiltradas` | Filtrado ocurre en memoria, no en SQL |
| `Pagination.razor` es HTML estático | Componente sin funcionalidad |
| Sin contador de total real | No se puede mostrar "Mostrando X de Y" |

### Flujo de datos DESPUÉS de la paginación

```
SQLite (tareas.db)
  └─ TareaSqliteRepository.GetTareasPaginadasAsync()
       └─ _context.Tareas.Where().CountAsync() + .Skip().Take().ToListAsync()
            └─ PaginatedResult<TareaModel> (items + metadata)
                 └─ TareaState.CargarPaginaAsync()
                      └─ ListaTareas (renderiza solo una página)
                           └─ Pagination.razor (botones dinámicos)
```

---

## Archivos Creados

### 1. `Models/PaginatedResult.cs`

Modelo genérico que encapsula el resultado de una consulta paginada con metadata.

```csharp
namespace TareasBlazor.Models
{
    public class PaginatedResult<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int TotalCount { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;

        public PaginatedResult(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
        }
    }
}
```

**Decisiones de diseño:**
- Genérico `<T>` para reutilizar en otras entidades si se necesita
- `IReadOnlyList<T>` para `Items` (consistencia con `TareaState.Tareas`)
- Propiedades calculadas (`TotalPages`, `HasPrevious`, `HasNext`) simplifican la UI

---

### 2. `Models/PaginationParams.cs`

Parámetros de entrada para las consultas paginadas con límites de seguridad.

```csharp
namespace TareasBlazor.Models
{
    public class PaginationParams
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        public int Page { get; set; } = 1;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = Math.Min(value, MaxPageSize);
        }
    }
}
```

**Decisiones de diseño:**
- `MaxPageSize = 50` para evitar consultas excesivamente grandes
- `PageSize` default = 10 (razonable para UX en tabla)
- Validación en el setter protege contra valores extremos

---

## Archivos Modificados

### 3. `Infrastructure/Interfaces/ITareaRepository.cs`

**Método agregado:**

```csharp
Task<PaginatedResult<TareaModel>> GetTareasPaginadasAsync(
    PaginationParams paginationParams,
    string? prioridad = null,
    string? estado = null,
    string? vencimiento = null);
```

**Por qué se agregan parámetros de filtro directamente en el repositorio:**
- El repositorio es el responsable de traducir filtros a SQL
- Mantiene la separación de capas (el State no genera consultas SQL)
- Permite combinar paginación + filtros en una sola consulta eficiente

---

### 4. `Infrastructure/Repositories/TareaSqliteRepository.cs`

**Implementación del método:**

```csharp
public async Task<PaginatedResult<TareaModel>> GetTareasPaginadasAsync(
    PaginationParams paginationParams,
    string? prioridad = null,
    string? estado = null,
    string? vencimiento = null)
{
    IQueryable<TareaModel> query = _context.Tareas;

    if (!string.IsNullOrEmpty(prioridad) && Enum.TryParse<Prioridad>(prioridad, out var p))
        query = query.Where(t => t.Prioridad == p);

    if (!string.IsNullOrEmpty(estado))
        query = estado == "Completadas"
            ? query.Where(t => t.Completada)
            : query.Where(t => !t.Completada);

    if (!string.IsNullOrEmpty(vencimiento))
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        query = vencimiento switch
        {
            "Vencidas" => query.Where(t => t.FechaVencimiento < today),
            "VencenHoy" => query.Where(t => t.FechaVencimiento == today),
            "ATiempo" => query.Where(t => t.FechaVencimiento > today),
            _ => query
        };
    }

    var totalCount = await query.CountAsync();

    var items = await query
        .OrderByDescending(t => t.Id)
        .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
        .Take(paginationParams.PageSize)
        .ToListAsync();

    return new PaginatedResult<TareaModel>(
        items.AsReadOnly(), totalCount,
        paginationParams.Page, paginationParams.PageSize);
}
```

**Por qué `CountAsync()` se ejecuta antes de `Skip/Take`:**
- Necesario para calcular `TotalPages` y mostrar información al usuario
- SQLite optimiza `COUNT(*)` internamente
- El orden (Count → Skip → Take) permite que EF Core genere SQL eficiente

**Por qué `OrderByDescending(t => t.Id)`:**
- Mantiene el mismo orden que `TareaState.Inicializar()` usaba antes
- Las tareas más recientes aparecen primero

---

### 5. `Shared/TareaState.cs`

**Cambios principales:**

```csharp
// ANTES:
private readonly List<TareaModel> _tareas = [];
public IReadOnlyList<TareaModel> Tareas => _tareas.AsReadOnly();

// DESPUÉS:
public PaginatedResult<TareaModel>? ResultadoPaginado { get; private set; }
public PaginationParams PaginacionActual { get; } = new();
public string? FiltroPrioridad { get; private set; }
public string? FiltroEstado { get; private set; }
public string? FiltroVencimiento { get; private set; }
```

**Nuevo método principal:**

```csharp
public async Task CargarPaginaAsync()
{
    IsLoading = true;
    NotificarCambio();

    ResultadoPaginado = await _repo.GetTareasPaginadasAsync(
        PaginacionActual, FiltroPrioridad, FiltroEstado, FiltroVencimiento);

    IsLoading = false;
    NotificarCambio();
}
```

**Métodos de filtro (resetean a página 1):**

```csharp
public async Task SetFiltroPrioridadAsync(string? prioridad)
{
    FiltroPrioridad = prioridad;
    PaginacionActual.Page = 1;
    await CargarPaginaAsync();
}

public async Task SetFiltroEstadoAsync(string? estado)
{
    FiltroEstado = estado;
    PaginacionActual.Page = 1;
    await CargarPaginaAsync();
}

public async Task SetFiltroVencimientoAsync(string? vencimiento)
{
    FiltroVencimiento = vencimiento;
    PaginacionActual.Page = 1;
    await CargarPaginaAsync();
}

public async Task IrAPaginaAsync(int page)
{
    PaginacionActual.Page = page;
    await CargarPaginaAsync();
}
```

**Por qué se resetea la página a 1 al cambiar filtros:**
- Si el usuario filtra y hay menos resultados, la página actual podría quedar fuera de rango
- UX consistente: siempre se empieza desde el inicio al cambiar criterios

---

### 6. `Components/Shared/Pagination.razor`

**Reemplazo completo del HTML estático por componente funcional:**

```razor
@if (TotalPages > 1)
{
    <nav aria-label="Navegación de páginas">
        <ul class="pagination mb-0">
            <li class="page-item @(HasPrevious ? "" : "disabled")">
                <button class="page-link" @onclick="PaginaAnterior"
                        disabled="@(!HasPrevious)" aria-label="Página anterior">
                    &laquo;
                </button>
            </li>

            @for (int i = 1; i <= TotalPages; i++)
            {
                var pageNum = i;
                <li class="page-item @(pageNum == CurrentPage ? "active" : "")">
                    <button class="page-link" @onclick="() => IrAPagina(pageNum)"
                            aria-current="@(pageNum == CurrentPage ? "page" : null)">
                        @pageNum
                    </button>
                </li>
            }

            <li class="page-item @(HasNext ? "" : "disabled")">
                <button class="page-link" @onclick="PaginaSiguiente"
                        disabled="@(!HasNext)" aria-label="Página siguiente">
                    &raquo;
                </button>
            </li>
        </ul>
    </nav>
}

@code {
    [Parameter] public int CurrentPage { get; set; }
    [Parameter] public int TotalPages { get; set; }
    [Parameter] public bool HasPrevious { get; set; }
    [Parameter] public bool HasNext { get; set; }
    [Parameter] public EventCallback<int> OnPageChanged { get; set; }

    private async Task IrAPagina(int page)
    {
        if (page != CurrentPage)
            await OnPageChanged.InvokeAsync(page);
    }

    private async Task PaginaAnterior()
    {
        if (HasPrevious)
            await OnPageChanged.InvokeAsync(CurrentPage - 1);
    }

    private async Task PaginaSiguiente()
    {
        if (HasNext)
            await OnPageChanged.InvokeAsync(CurrentPage + 1);
    }
}
```

**Decisiones de diseño:**
- Solo se muestra si `TotalPages > 1` (no mostrar paginación con 1 página)
- Usa `button` en vez de `<a href="#">` para evitar navegación no deseada en Blazor Server
- `var pageNum = i` dentro del loop para capturar correctamente la variable en el closure
- Atributos ARIA para accesibilidad

---

### 7. `Components/Shared/ListaTareas.razor`

**Cambios en el template:**

```razor
<!-- ANTES: -->
@foreach (var tarea in TareasFiltradas)
{
    <TareaItem @key="tarea.IdPublic" Tarea="tarea" ... />
}

<!-- DESPUÉS: -->
@foreach (var tarea in tareaState.ResultadoPaginado?.Items ?? [])
{
    <TareaItem @key="tarea.IdPublic" Tarea="tarea" ... />
}
```

**Conexión con Pagination:**

```razor
<Pagination CurrentPage="@(tareaState.ResultadoPaginado?.Page ?? 1)"
            TotalPages="@(tareaState.ResultadoPaginado?.TotalPages ?? 1)"
            HasPrevious="@(tareaState.ResultadoPaginado?.HasPrevious ?? false)"
            HasNext="@(tareaState.ResultadoPaginado?.HasNext ?? false)"
            OnPageChanged="OnPageChanged" />
```

**Handler nuevo:**

```csharp
private async Task OnPageChanged(int page)
{
    await tareaState.IrAPaginaAsync(page);
}
```

**Filtros ahora usan el State directamente:**

```csharp
// ANTES:
private void OnPrioridadChanged(string valor) { _prioridadSeleccionada = valor; }

// DESPUÉS:
private async Task OnPrioridadChanged(string valor) {
    await tareaState.SetFiltroPrioridadAsync(valor);
}
```

**Se eliminó:**
- La property `TareasFiltradas` (ya no es necesaria, el filtro está en DB)
- Las variables locales `_prioridadSeleccionada`, `_completadaSeleccionada`, `_vencimientoSeleccionado`

---

## Orden de Ejecución

| Paso | Archivo | Dependencias |
|---|---|---|
| 1 | `Models/PaginatedResult.cs` | Ninguna |
| 2 | `Models/PaginationParams.cs` | Ninguna |
| 3 | `Infrastructure/Interfaces/ITareaRepository.cs` | Paso 1, 2 |
| 4 | `Infrastructure/Repositories/TareaSqliteRepository.cs` | Paso 3 |
| 5 | `Shared/TareaState.cs` | Paso 4 |
| 6 | `Components/Shared/Pagination.razor` | Ninguna (UI independiente) |
| 7 | `Components/Shared/ListaTareas.razor` | Paso 5, 6 |

---

## SQL Generado por EF Core

Para una consulta con filtros y paginación, EF Core genera:

```sql
-- Conteo total
SELECT COUNT(*)
FROM Tareas
WHERE Prioridad = 'Alta' AND Completada = 0;

-- Página de resultados
SELECT Id, IdPublic, Titulo, Descripcion, Completada, FechaVencimiento, Prioridad, Imagen, Categoria
FROM Tareas
WHERE Prioridad = 'Alta' AND Completada = 0
ORDER BY Id DESC
LIMIT 10 OFFSET 0;
```

**Efficiency:** Solo se transfieren 10 registros por consulta en vez de toda la tabla.

---

## Cómo Extender

### Cambiar items por página

```csharp
// En TareaState o donde se instancie PaginationParams
PaginacionActual.PageSize = 20;  // Máximo 50 (limitado en PaginationParams)
```

### Agregar nuevo filtro

1. Agregar parámetro `string?` al método `GetTareasPaginadasAsync`
2. Agregar cláusula `Where()` en `TareaSqliteRepository`
3. Agregar propiedad y método `SetFiltroXxxAsync()` en `TareaState`
4. Conectar en el componente Razor correspondiente

---

## Consideraciones

- **SQLite y DateOnly:** El `DateOnly` se convierte a `DateTime` en la DB mediante conversión configurada en `AppDbContext.OnModelCreating`
- **Thread safety:** `SemaphoreSlim` en `TareaState` se mantiene para proteger la inicialización
- **Memory:** Con paginación, solo ~10 objetos `TareaModel` están en memoria por request (vs toda la tabla antes)
- **Compatibilidad:** Los métodos existentes (`GetTareasAsync`, etc.) se mantienen para uso en otras partes de la app (dashboard, stats)
