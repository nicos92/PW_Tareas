# Auditoría de Código — UNIDAD 1: Componentes Blazor

**Proyecto:** `PW_Tareas` · **Capa UI:** `src/Tareas.Web` · **Arquitectura:** Blazor Web App (.NET) con render `InteractiveServer` (`Components/Routes.razor:1`, `Program.cs:18-19,55-56`).

---

## Estado de Cumplimiento - UNIDAD 1

### 1. Componentes Razor reutilizables y encapsulados
- **Estado: CUMPLE** (se supera ampliamente el mínimo de 6; hay **17 componentes** en `Shared` + 5 en `Layout`).
- **Evidencia / Ausencia:**
  - **Ruta de la carpeta:** `src/Tareas.Web/Components/Shared/` (y `src/Tareas.Web/Components/Layout/`)
  - **Archivos y líneas:**
    - `CardSection.razor` (Líneas 1-26) — `[Parameter]` `Title`, `CssClass`, `HeaderContent`, `ChildContent` (`RenderFragment`).
    - `TareaForm.razor` (Líneas 59-90) — parámetros `Model`, `Categorias`, `EventCallback OnValidSubmit`, `ChildContent`; API pública `HasValidationErrors` (Línea 89).
    - `TareaItem.razor` (Líneas 63-91) — `[Parameter] Tarea` + 5 `EventCallback` (`OnVer`, `OnCompletar`, `OnDarDeBaja`, `OnRestaurar`, `OnEditar`).
    - `FilterBar.razor` (Líneas 31-74) — parámetros y 4 `EventCallback<string>` (Líneas 40-43).
    - `Pagination.razor` (Líneas 30-54) — parámetros + `EventCallback<int> OnPageChanged` (Línea 35).
    - `ConfirmDialog.razor` (Líneas 30-56) — estado interno `_procesando` + `OnConfirm`/`OnCancel` (Líneas 37-38).
    - `SelectFilter.razor` (Líneas 15-31) — encapsula `OpcionFiltro` como record anidado (Línea 16) y `EventCallback<string>` (Línea 25).
    - `ImageUploader.razor` + `ImageUploader.razor.cs` (Líneas 8-90) — parámetros (Líneas 10-12), estado privado (`archivoCargado`, `imagenBase64`, `tamanoExcedido`, Líneas 14-18) y evento tipado `ImagenChangedEventArgs` (Líneas 84-90).
    - `EmptyState.razor` (9-13), `CharCounter.razor` (5-12), `FormActions.razor` (6-13), `LoadingSpinner.razor` (7-9), `StatsCard.razor` (8-14), `TableSkeleton.razor` (25-28), `TareaCard.razor` (62-65), `Breadcrum.razor.cs` (10-34).
    - **Layout:** `MainLayout.razor` (1-17), `NavMenu.razor` (1-43), `NavMenuOffcanvas.razor` (1-13), `NavScroll.razor` (1-27), `SkipLink.razor` (1-3), `StatsPanel.razor` (1-27).
  - **Detalle de encapsulamiento:** cada componente aísla su marcado y expone únicamente `[Parameter]`/`[EventCallback]`, sin acceder a servicios de dominio directamente (p. ej. `CardSection`, `Pagination`, `CharCounter` son 100% agnósticos). La composición se evidencia en el uso de `ChildContent` dentro de `TareaForm`/`CardSection` y en su consumo desde `NuevaTarea.razor:8-14` y `EditarTarea.razor:15-21`.

---

### 2. Formularios con EditForm y validación avanzada
- **Estado: CUMPLE**
- **Evidencia / Ausencia:**
  - **Ruta de la carpeta:** `src/Tareas.Web/Components/Shared/`, `src/Tareas.Web/Components/Pages/` y `src/Tareas.Application/`
  - **Archivos y líneas:**
    - `Shared/TareaForm.razor` — `<EditForm Model="Model" OnValidSubmit="OnValidSubmit">` (Líneas 1-2), `<DataAnnotationsValidator />` (Línea 3), `<ValidationSummary />` (Línea 4), `<ValidationMessage For="() => Model.Titulo/Descripcion/CategoryId" />` (Líneas 12, 21, 53).
    - `Pages/Categorias.razor` — `EditForm` (Línea 25), `DataAnnotationsValidator` (Línea 26), `ValidationSummary` (Línea 27), `ValidationMessage` (Líneas 33, 41).
    - `Application/Dtos/TareaDto.cs` (Líneas 15-56) — `[Required]`, `[StringLength]`, `[MaxLength]`, `[EnumDataType]`, `[DataType]`.
    - `Application/Dtos/CategoryDto.cs` (Líneas 12-22) — `[Required]` + `[StringLength]` + `[MaxLength]`.
    - `Application/Validation/FutureDateAttribute.cs` (Líneas 5-44) — **validación personalizada** (`ValidationAttribute`) que soporta `DateTime` y `DateOnly`.
    - `Application/Validation/ArchivoImagenAttribute.cs` (Líneas 5-32) — **validación personalizada** por extensión de archivo.
    - `Shared/TareaForm.razor` (Líneas 72-89) — validación en vivo manual con `EditContext.NotifyFieldChanged` y `GetValidationMessages()`.
    - `Shared/ImageUploader.razor.cs` (Líneas 27-40) — validación de tamaño y extensión del archivo en UI.
  - **Detalle de validación:** se combinan **Data Annotations** (`Required`, `StringLength`, `MaxLength`, `EnumDataType`), **atributos personalizados** (`FutureDateAttribute`, `ArchivoImagenAttribute`) y **validación cruzada entre campos** mediante `IValidatableObject.Validate` en `TareaDto.cs:48-56` (impide prioridad `Alta` con vencimiento a más de 7 días). No se usa FluentValidation, pero el requisito es alternativo ("Data Annotations, FluentValidation **o** validaciones personalizadas"), y aquí se usan dos de las tres.

---

### 3. Routing con parámetros y navegación programática
- **Estado: CUMPLE**
- **Evidencia / Ausencia:**
  - **Routing con parámetros:**
    - `Pages/Tarea.razor` (Línea 1) — `@page "/listaDeTareas/tarea/{id}"`; recepción en `Tarea.razor.cs` (Líneas 10-11 `[Parameter] public string id`) y uso en `OnParametersSetAsync` (Líneas 19-24).
    - `Pages/EditarTarea.razor` (Línea 1) — `@page "/listaDeTareas/editarTarea/{Id}"`; recepción en `EditarTarea.razor.cs` (Líneas 10-11) y carga por Id (Líneas 17-25, 50-55).
    - Rutas estáticas complementarias: `Pages/ListaTareas.razor:1`, `Pages/NuevaTarea.razor:1`, `Pages/Categorias.razor:1`, `Pages/Configuracion.razor:1`, `Pages/Home.razor:1`, `Pages/Error.razor:1`.
    - Enrutador global: `Components/Routes.razor` (Líneas 3-7) con `Router`/`RouteView`/`FocusOnNavigate`.
  - **Navegación programática:**
    - `Pages/ListaTareas.razor.cs` — `NavigationManager.NavigateTo($"/listaDeTareas/tarea/{tarea.IdPublic}")` (Líneas 29-32), edición (Líneas 61-64) y alta (Líneas 66-69).
    - `Features/Tareas/TareaFormPageBase.cs` — inyección de `NavigationManager` (Líneas 18-19) y `Cancelar()` (Línea 55).
    - `Pages/NuevaTarea.razor.cs` (Línea 19) y `Pages/EditarTarea.razor.cs` (Línea 56) — redirección tras submit.
    - `Shared/Breadcrum.razor.cs` — inyección (Línea 13), lectura de URL con `NavigationManager.Uri`/`ToBaseRelativePath` (Líneas 51-52) y suscripción a `LocationChanged` (Líneas 33, 44-47, 119).

---

### 4. State Service con notificación reactiva
- **Estado: CUMPLE**
- **Evidencia / Ausencia:**
  - **Servicio de Estado:**
    - `Features/Tareas/TareaEstado.cs` (Líneas 14-207) — clase `sealed`, inyección de casos de uso (Líneas 28-46), propiedades de estado de UI (Líneas 48-54), **`public event Action? OnChange;`** (Línea 55), mutadores que recargan/notifican (Líneas 88-103, 105-157, 159-198) y **`NotificarCambio() => OnChange?.Invoke()`** (Línea 207).
    - `Features/Categorias/CategoriaEstado.cs` (Líneas 11-98) — estado del listado (Líneas 36-37), **`public event Action? OnChange;`** (Línea 39), notificación tras carga/CRUD (Líneas 66-75, 79-95) y `NotificarCambio()` (Línea 97).
    - `Features/Tema/ThemeService.cs` (Líneas 8-38) — servicio de estado adicional con **`public event Action? OnThemeChanged;`** (Línea 14) e invocación (Línea 31).
    - **Registro DI:** `Program.cs` (Líneas 24-26) — `AddScoped<ThemeService>()`, `AddScoped<TareaEstado>()`, `AddScoped<CategoriaEstado>()`.
  - **Suscripción/Notificación en componente:**
    - `Features/Tareas/TareaEstadoComponentBase.cs` (Líneas 10-27) — `[Inject] TareaEstado` (Líneas 12-13), **`TareaEstado.OnChange += StateHasChanged`** (Línea 19) y baja en `Dispose` (Línea 26). Es la base de `Pages/ListaTareas.razor:2`, `Pages/Home.razor:2` y `Layout/StatsPanel.razor:1`.
    - `Pages/Categorias.razor.cs` (Líneas 11-12, 23-27) — inyección y **`CategoriaEstado.OnChange += StateHasChanged`** (Línea 25); desuscripción en `Dispose` (Líneas 119-122).
    - `Layout/StatsPanel.razor` (Líneas 1, 7-25) — consume `TareaEstado.Estadisticas` y se re-renderiza por la suscripción heredada, demostrando reactividad cruzada entre componentes.
    - `Pages/Configuracion.razor` (Líneas 2, 13-17) — consume `ThemeService` para cambiar el tema.

---

## Observaciones menores (no afectan el cumplimiento)

- `ThemeService.OnThemeChanged` se expone pero ningún componente se suscribe; la reactividad del tema se resuelve vía JS. Es correcto, aunque si se desea consistencia con el patrón, convendría que algún componente (p. ej. un indicador de tema) escuche el evento.
- `TareaEstado` notifica desde métodos `async` sin `InvokeAsync`; en Blazor Server `StateHasChanged` ya se marshal al dispatcher, pero para escenarios multi-hilo convendría documentarlo. No es un incumplimiento del requisito.

**Conclusión:** los 4 requisitos de la UNIDAD 1 se cumplen de forma rigurosa, con evidencia directa y trazable en el código.
