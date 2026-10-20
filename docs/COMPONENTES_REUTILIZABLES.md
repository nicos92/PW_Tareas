# Componentes reutilizables — Tareas.Web

Inventario de los componentes reutilizables de `src/Tareas.Web`, sus dependencias y los
patrones de desacoplamiento aplicados. Objetivo: que cualquier componente nuevo sea
reutilizable sin conocer el dominio de tareas salvo mediante parámetros/contratos.

## Estructura

```
Components/
├── Shared/      → componentes reutilizables (dominio-agnósticos salvo indicación)
├── Layout/      → armazón de la aplicación
├── Features/    → servicios y bases de componente por feature
│   ├── Tareas/      TareaEstado, TareaEstadoComponentBase, TareaFormPageBase
│   ├── Tema/        ThemeService
│   └── Archivos/    SubidaArchivoFactory
└── Pages/       → páginas (rutas)
Helpers/
    PrioridadHelper.cs
    EstadoTareaHelper.cs
```

## Inventario de componentes reutilizables (`Components/Shared`)

| Componente | Propósito | Parámetros principales | Dependencias de dominio |
|---|---|---|---|
| `CardSection` | Card Bootstrap con header + body | `Title`, `HeaderContent`, `ChildContent`, `CssClass`, `AriaLabel`, `HeaderId` | Ninguna |
| `CharCounter` | Contador `actual/máximo` con estado de alerta | `Value`, `MaxLength`, `WarnAt` (default 0.9) | Ninguna |
| `SelectFilter` | `<select>` genérico con label y callback | `Id`, `Label`, `Options` (`OpcionFiltro(Valor, Etiqueta)`), `Value`, `EmptyLabel`, `Disabled`, `OnChanged` | Ninguna |
| `FilterBar` | Barra de 3 filtros + contador de resultados | `IsLoading`, valores seleccionados, callbacks, `TotalTareas`, `Legend`, `AriaLabel` | Opciones generadas desde `Prioridad`, `EstadoFiltro`, `VencimientoFiltro` (enums, sin `<option>` hardcodeados) |
| `FormActions` | Botones Guardar/Cancelar de formularios | `IsSubmitDisabled`, `SubmitText`, `CancelText`, `SubmitCssClass`, `CancelCssClass`, `OnCancel` | Ninguna |
| `LoadingSpinner` | Spinner centrado de carga | `Mensaje` | Ninguna |
| `TableSkeleton` | Skeleton de tabla en carga | `Columnas` (lista de títulos), `Filas` | Ninguna |
| `EmptyState` | Mensaje de "sin datos" accesible (`aria-live`) | `Mensaje`, `Icono`, `ClaseTexto` | Ninguna |
| `ConfirmDialog` | Modal de confirmación con estado "procesando" | `IsVisible`, `Title`, `Message`, `TargetName`, `ConfirmText`, `CancelText`, `OnConfirm`, `OnCancel` | Ninguna |
| `Pagination` | Paginación prev/siguiente | `CurrentPage`, `TotalPages`, `HasPrevious`, `HasNext`, `OnPageChanged` | Ninguna |
| `StatsCard` | Tarjeta de estadística título+valor | `Title`, `Value`, `BackgroundClass`, `ValueCssClass` | Ninguna |
| `TareaForm` | Formulario EditForm de tarea con validación | `Model` (`TareaDto`), `OnValidSubmit`, `ChildContent`; expone `HasValidationErrors` | `TareaDto`, enums `Prioridad`/`Categoria` |
| `TareaItem` | Fila de tabla de tarea (checkbox + acciones) | `Tarea`, `OnVer`, `OnCompletar`, `OnEliminar`, `OnEditar` | `TareaDto`, `PrioridadHelper` |
| `TareaCard` | Tarjeta de detalle de tarea | `Tarea` | `TareaDto`, `PrioridadHelper`, `EstadoTareaHelper` |
| `ImageUploader` | Subida + preview de imagen con límite de tamaño | `CurrentImagen`, `OnImagenChanged`, `MaxFileSize`; expone `Archivo`, `TamanoExcedido` | Ninguna (adaptador `SubidaArchivoFactory` hacia Application) |
| `Breadcrum` | Breadcrumb derivado de la URL | `SegmentNameResolver` (`Func<string, Task<string?>>?`) | Ninguna — la resolución de GUID→título la aporta la página |

### Layout

| Componente | Propósito | Acoplamiento |
|---|---|---|
| `MainLayout` | Armazón: `SkipLink` + `NavMenu` + body + `StatsPanel` | `TareaEstado` vía `StatsPanel` |
| `NavMenu` / `NavMenuOffcanvas` / `NavScroll` | Navegación lateral y offcanvas | Ninguno |
| `SkipLink` | Enlace de salto accesible al contenido | Ninguno |
| `StatsPanel` | Panel de estadísticas del sidebar; la sección "Estado" reutiliza `StatsCard` | `TareaEstado` (via `TareaEstadoComponentBase`) |

### Bases y helpers

| Tipo | Propósito |
|---|---|
| `TareaEstado` | Estado de presentación scoped de la lista de tareas (filtros, paginación, carga); delega en casos de uso de Application. |
| `TareaEstadoComponentBase` | Suscribe `OnChange → StateHasChanged` y da de baja en `Dispose`; llama a `Inicializar()`. Usar en toda página/componente que observe `TareaEstado`. |
| `TareaFormPageBase` | Base de páginas crear/editar: inyecciones (`NavigationManager`, `TareaEstado`, `OpcionesAlmacenamiento`), `MaxFileSize`, refs de `TareaForm`/`ImageUploader`, `CrearSubida()`, `ImagenInvalida`, `Cancelar()`. |
| `PrioridadHelper.GetClase(Prioridad)` | Clase CSS de badge por prioridad (`badge-alta/media/baja`). |
| `EstadoTareaHelper` | `GetEstado(dto)` → `EstadoVisual` { `Completada`, `Vencida`, `PorVencer`, `Pendiente` } (umbral 2 días) + `GetClaseBadge`, `GetEtiqueta`, `GetClaseSuperficie`. Única fuente de verdad del estado visual; `TareaCard` la consume. |

## Patrones de desacoplamiento aplicados

1. **Resolver por parámetro en vez de inyección de dominio**
   `Breadcrum` ya no inyecta `TareaEstado`. Las páginas con GUID en la ruta pasan
   `SegmentNameResolver="ResolverTituloTarea"`:
   ```razor
   <Breadcrum SegmentNameResolver="ResolverTituloTarea" />
   ```
   ```csharp
   private async Task<string?> ResolverTituloTarea(string segmento)
   {
       var tarea = await TareaEstado.ObtenerPorIdAsync(segmento);
       return tarea?.Titulo;
   }
   ```
   Sin resolver, el breadcrumb muestra el segmento tal cual. Reutilizable en cualquier módulo.

2. **Opciones de filtro desde enums, no hardcodeadas**
   `FilterBar` genera las opciones con `Enum.GetValues<>()` (`Prioridad`, `EstadoFiltro`,
   `VencimientoFiltro`). Agregar un valor al enum lo refleja en la UI sin tocar el componente.
   `SelectFilter` es el componente genérico reutilizable para cualquier `<select>` con
   opción vacía ("Todas").

3. **Componentes pequeños con una responsabilidad**
   `CharCounter`, `LoadingSpinner`, `TableSkeleton`, `EmptyState`, `FormActions` y
   `CardSection` extraen patrones que estaban duplicados en páginas.

4. **Lógica de dominio en helpers, no en markup**
   El cálculo vencida/por-vencer vivía embebido en `TareaCard.razor`; ahora está en
   `EstadoTareaHelper` y es testeable/reutilizable.

5. **Base de página para el flujo crear/editar**
   `NuevaTarea` y `EditarTarea` heredan de `TareaFormPageBase`; solo conservan su modelo y
   `ValidSubmit`. Un nuevo formulario con imagen reutiliza la base.

## Ejemplos de uso

```razor
@* Card con header personalizado *@
<CardSection Title="Mi sección" CssClass="col-12 mb-3">
    ...contenido...
</CardSection>

@* Filtro genérico *@
<SelectFilter Id="mi-filtro" Label="Estado" Options="opciones" Value="@valor"
              EmptyLabel="Todos" OnChanged="OnChanged" />

@* Skeleton y vacío *@
<TableSkeleton Columnas='@new[] { "Nombre", "Valor" }' Filas="4" />
<EmptyState Mensaje="No hay elementos." Icono="bi-inbox" />

@* Acciones de formulario *@
<FormActions IsSubmitDisabled="@invalido" OnCancel="Cancelar" />
```

## Convenciones

- Accesibilidad: `aria-label`/`aria-live` en interactivos; botones de icono con label accesible.
- Los componentes `Shared` no deben inyectar `TareaEstado`; si necesitan datos de dominio,
  reciben DTOs o callbacks como parámetros.
- Los textos visibles van por parámetro con default razonable (componentes en español).
- Estados visuales (badges, tintes) salen de `PrioridadHelper`/`EstadoTareaHelper`, nunca
  clases CSS duplicadas en markup.
