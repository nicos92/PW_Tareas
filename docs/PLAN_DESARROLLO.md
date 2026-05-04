# Plan de Desarrollo - TareasBlazor

## Estado actual del proyecto

- **Framework:** .NET 8.0 Blazor Interactive Server
- **Base de datos:** SQLite (EF Core 8.0.26) con DbContext registrado pero sin usar
- **Almacenamiento activo:** `TareaProtectedLocalStorageRepository` (server-side encrypted storage)
- **Idioma:** Español

---

## FASE 1: Componentes Reutilizables

### 1.1 ImageUploader.razor

**Ubicación:** `Components/Shared/ImageUploader.razor`

**Propósito:** Extraer la lógica de carga de imagen duplicada en `NuevaTarea.razor` y `EditarTarea.razor`.

**Parámetros:**
- `CurrentImagen` (string?) - Imagen existente a mostrar (solo editar)
- `OnImagenChanged` (EventCallback) - Notifica cambios al padre
- `MaxFileSize` (long, default: 1MB) - Tamaño máximo configurable
- `ValidationFor` (Expression) - Para validación del campo

**Estado interno:**
- `archivoCargado` (IBrowserFile?)
- `imagenBase64` (string?)
- `tamanoExcedido` (bool)
- `inputFileKey` (int) - Para resetear el InputFile

**Comportamiento:**
- Muestra preview base64 si el archivo es válido
- Muestra imagen existente si no hay archivo nuevo y `CurrentImagen` tiene valor
- Botón "Limpiar imagen" que resetea todo
- Notifica al padre vía `OnImagenChanged` con `ImagenChangedEventArgs`

**Código del componente:**

```razor
<div class="mb-3">
    <label class="form-label">Suba una imagen:</label>
    <InputFile OnChange="CargarArchivo" accept="image/*" class="form-control" @key="inputFileKey" />
    @if (archivoCargado is not null)
    {
        <div class="alert @TipoAlerta mt-2">
            @if (tamanoExcedido)
            {
                <p>El archivo de imagen tiene que pesar menos de @FormatoTamano(maxFileSize)</p>
            }
            @if (!tamanoExcedido)
            {
                <div class="container-fluid">
                    <div class="">
                        <img src="@imagenBase64" alt="Vista previa" style="width: 300px"/>
                    </div>
                </div>
            }
            <p class="mb-1">Archivo: @archivoCargado.Name</p>
            <p class="mb-0">Tamaño: @FormatoTamano(archivoCargado.Size)</p>
            <button type="button" class="btn btn-sm btn-outline-danger mt-2" @onclick="Limpiar">Limpiar imagen</button>
        </div>
    }
    else if (!string.IsNullOrEmpty(CurrentImagen))
    {
        <div class="mt-2">
            <img src="@CurrentImagen" alt="Imagen actual" style="width: 300px"/>
            <div>
                <button type="button" class="btn btn-sm btn-outline-danger mt-2" @onclick="Limpiar">Eliminar imagen actual</button>
            </div>
        </div>
    }
</div>

@code {
    [Parameter] public string? CurrentImagen { get; set; }
    [Parameter] public EventCallback<ImagenChangedEventArgs> OnImagenChanged { get; set; }
    [Parameter] public long MaxFileSize { get; set; } = 1024 * 1024;

    private IBrowserFile? archivoCargado;
    private string? imagenBase64;
    private bool tamanoExcedido;
    private int inputFileKey;

    private string TipoAlerta => tamanoExcedido ? "alert-danger" : "alert-success";

    public IBrowserFile? Archivo => archivoCargado;
    public bool TamanoExcedido => tamanoExcedido;

    private async Task CargarArchivo(InputFileChangeEventArgs e)
    {
        archivoCargado = e.File;
        tamanoExcedido = archivoCargado.Size > MaxFileSize;
        imagenBase64 = null;

        if (!tamanoExcedido)
        {
            using var stream = archivoCargado.OpenReadStream(MaxFileSize);
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            imagenBase64 = $"data:{archivoCargado.ContentType};base64,{Convert.ToBase64String(memoryStream.ToArray())}";
        }

        await OnImagenChanged.InvokeAsync(new ImagenChangedEventArgs
        {
            Archivo = archivoCargado,
            ImagenBase64 = imagenBase64,
            TamanoExcedido = tamanoExcedido
        });

        StateHasChanged();
    }

    private async Task Limpiar()
    {
        archivoCargado = null;
        imagenBase64 = null;
        tamanoExcedido = false;
        inputFileKey++;

        await OnImagenChanged.InvokeAsync(new ImagenChangedEventArgs
        {
            Archivo = null,
            ImagenBase64 = null,
            TamanoExcedido = false,
            LimpiarImagenExistente = true
        });

        StateHasChanged();
    }

    private string FormatoTamano(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }

    public class ImagenChangedEventArgs
    {
        public IBrowserFile? Archivo { get; set; }
        public string? ImagenBase64 { get; set; }
        public bool TamanoExcedido { get; set; }
        public bool LimpiarImagenExistente { get; set; }
    }
}
```

**Modificar NuevaTarea.razor:**
- Reemplazar el bloque de imagen (líneas 45-68) por `<ImageUploader OnImagenChanged="OnImagenChanged" MaxFileSize="1048576" @ref="imageUploader" />`
- Eliminar: `archivoCargado`, `imagenBase64`, `tamanoExcedido`, `inputFileKey`, `CargarArchivo`, `LimpiarImagen`, `FormatoTamano`, `TipoAlerta`
- Agregar referencia `@ref` al componente `ImageUploader`
- En `ValidSubmit`, usar `imageUploader.Archivo` y `imageUploader.TamanoExcedido`
- Mantener solo `GuardarArchivoAsync` y `Cancelar`

**Modificar EditarTarea.razor:**
- Reemplazar el bloque de imagen (líneas 52-82) por `<ImageUploader CurrentImagen="tareaModel.Imagen" OnImagenChanged="OnImagenChanged" MaxFileSize="1048576" @ref="imageUploader" />`
- Misma limpieza de código duplicado
- En `LimpiarImagen`, ya no es necesario limpiar campos internos, solo `tareaModel.Imagen = string.Empty`

---

### 1.2 TareaForm.razor

**Ubicación:** `Components/Shared/TareaForm.razor`

**Propósito:** Extraer los campos comunes del formulario (título, descripción, completada, fecha, prioridad) que están duplicados en NuevaTarea y EditarTarea.

**Parámetros:**
- `Model` (TareaModel) - El modelo del formulario (required)
- `OnValidSubmit` (EventCallback) - Submit handler
- `IsSubmitting` (bool) - Estado de carga opcional
- `ChildContent` (RenderFragment) - Para contenido adicional (imagen, botones custom)

**Campos:**
- Titulo (InputText)
- Descripcion (InputTextArea)
- Completada (InputCheckbox)
- FechaVencimiento (InputDate)
- Prioridad (InputSelect con enum)

```razor
<EditForm Model="Model" OnValidSubmit="OnValidSubmit" class="container">
    <DataAnnotationsValidator />
    <ValidationSummary />

    <div class="mb-3">
        <label class="form-label">Título:</label>
        <InputText @bind-Value="Model.Titulo" class="form-control" />
        <ValidationMessage For="() => Model.Titulo" />
    </div>

    <div class="mb-3">
        <label class="form-label">Descripción:</label>
        <InputTextArea @bind-Value="Model.Descripcion" class="form-control" />
        <ValidationMessage For="() => Model.Descripcion" />
    </div>

    <div class="mb-3">
        <label class="form-check-label">Completada:</label>
        <InputCheckbox @bind-Value="Model.Completada" class="form-check-input" />
    </div>

    <div class="mb-3">
        <label class="form-label">Fecha de Vencimiento:</label>
        <InputDate @bind-Value="Model.FechaVencimiento" class="form-control" />
    </div>

    <div class="mb-3">
        <label class="form-label">Prioridad:</label>
        <InputSelect @bind-Value="Model.Prioridad" class="form-select">
            @foreach (var prioridad in Enum.GetValues<Prioridad>())
            {
                <option value="@prioridad">@prioridad</option>
            }
        </InputSelect>
    </div>

    @ChildContent
</EditForm>

@code {
    [Parameter, EditorRequired] public TareaModel Model { get; set; } = default!;
    [Parameter, EditorRequired] public EventCallback OnValidSubmit { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
```

**Modificar NuevaTarea.razor y EditarTarea.razor:**
- Reemplazar todo el `<EditForm>` por `<TareaForm Model="tareaModel" OnValidSubmit="ValidSubmit">` con `ChildContent` que contenga solo el ImageUploader y los botones

---

### 1.3 FilterBar.razor

**Ubicación:** `Components/Shared/FilterBar.razor`

**Propósito:** Extraer los filtros de prioridad y estado de ListaTareas.razor.

**Parámetros:**
- `PrioridadSeleccionada` (string)
- `EstadoSeleccionado` (string)
- `TotalTareas` (int) - Para mostrar contador
- `OnPrioridadChanged` (EventCallback<string>)
- `OnEstadoChanged` (EventCallback<string>)

**Opciones de prioridad:** Todas, Baja, Media, Alta
**Opciones de estado:** Todas, Completadas, Pendientes

```razor
<div class="row mb-3">
    <div class="col-md-4">
        <label class="form-label">Filtrar por prioridad:</label>
        <select class="form-select" value="@PrioridadSeleccionada" @onchange="OnPrioridadChangedHandler">
            <option value="">Todas</option>
            <option value="Baja">Baja</option>
            <option value="Media">Media</option>
            <option value="Alta">Alta</option>
        </select>
    </div>
    <div class="col-md-4">
        <label class="form-label">Filtrar por estado:</label>
        <select class="form-select" value="@EstadoSeleccionado" @onchange="OnEstadoChangedHandler">
            <option value="">Todas</option>
            <option value="Completadas">Completadas</option>
            <option value="Pendientes">Pendientes</option>
        </select>
    </div>
    <div class="col-md-4 d-flex align-items-end">
        <span class="text-muted">Mostrando: @TotalTareas tarea@(TotalTareas != 1 ? "s" : "")</span>
    </div>
</div>

@code {
    [Parameter] public string PrioridadSeleccionada { get; set; } = string.Empty;
    [Parameter] public string EstadoSeleccionado { get; set; } = string.Empty;
    [Parameter] public int TotalTareas { get; set; }
    [Parameter] public EventCallback<string> OnPrioridadChanged { get; set; }
    [Parameter] public EventCallback<string> OnEstadoChanged { get; set; }

    private async Task OnPrioridadChangedHandler(ChangeEventArgs e)
    {
        PrioridadSeleccionada = e.Value?.ToString() ?? string.Empty;
        await OnPrioridadChanged.InvokeAsync(PrioridadSeleccionada);
    }

    private async Task OnEstadoChangedHandler(ChangeEventArgs e)
    {
        EstadoSeleccionado = e.Value?.ToString() ?? string.Empty;
        await OnEstadoChanged.InvokeAsync(EstadoSeleccionado);
    }
}
```

---

### 1.4 ConfirmDialog.razor

**Ubicación:** `Components/Shared/ConfirmDialog.razor`

**Propósito:** Modal de confirmación genérico reemplazando el inline de ListaTareas.

**Parámetros:**
- `IsVisible` (bool)
- `Title` (string, default: "Confirmar eliminación")
- `Message` (string)
- `ConfirmText` (string, default: "Eliminar")
- `CancelText` (string, default: "Cancelar")
- `OnConfirm` (EventCallback)
- `OnCancel` (EventCallback)

```razor
@if (IsVisible)
{
    <div class="modal fade show d-block" tabindex="-1" style="background: rgba(0,0,0,0.5);">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">@Title</h5>
                    <button type="button" class="btn-close" @onclick="OnCancel"></button>
                </div>
                <div class="modal-body">
                    <p>@Message</p>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" @onclick="OnCancel">@CancelText</button>
                    <button type="button" class="btn btn-danger" @onclick="OnConfirm">@ConfirmText</button>
                </div>
            </div>
        </div>
    </div>
}

@code {
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public string Title { get; set; } = "Confirmar eliminación";
    [Parameter, EditorRequired] public string Message { get; set; } = string.Empty;
    [Parameter] public string ConfirmText { get; set; } = "Eliminar";
    [Parameter] public string CancelText { get; set; } = "Cancelar";
    [Parameter] public EventCallback OnConfirm { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
}
```

---

## FASE 2: Corrección de Bugs

### 2.1 Doble suscripción en ListaTareas.razor

**Archivo:** `Components/Shared/ListaTareas.razor`

**Problema:** `tareaState.OnChange += StateHasChanged` se llama en `OnInitialized()` Y en `OnAfterRenderAsync()`, causando doble suscripción y posible memory leak.

**Solución:** Eliminar la suscripción de `OnAfterRenderAsync`, dejar solo en `OnInitialized`.

```diff
  protected override void OnInitialized()
  {
      tareaState.OnChange += StateHasChanged;
+     tareaState.Inicializar().ConfigureAwait(false);
  }

- protected override async Task OnAfterRenderAsync(bool firstRender)
- {
-     if (firstRender)
-     {
-         tareaState.OnChange += StateHasChanged;
-         await tareaState.Inicializar();
-     }
- }
```

---

### 2.2 Implementar PesoArchivoAttribute

**Archivo:** `Validation/ArchivoValidationAttribute.cs`

**Problema:** `PesoArchivoAttribute.IsValid` siempre retorna `ValidationResult.Success` (stub).

**Solución:** Implementar validación real.

```csharp
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class PesoArchivoAttribute : ValidationAttribute
{
    public long MaxBytes { get; set; } = 1024 * 1024; // 1 MB por defecto

    public PesoArchivoAttribute()
    {
        ErrorMessage = "El archivo no debe superar {0}.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IBrowserFile archivo)
        {
            if (archivo.Size > MaxBytes)
            {
                return new ValidationResult(FormatErrorMessage(MaxBytes.ToString("N0") + " bytes"), new[] { validationContext.MemberName! });
            }
        }
        return ValidationResult.Success;
    }
}
```

**Nota:** Esta validación es difícil de aplicar directamente al string `Imagen` del modelo porque la validación de datos ocurre en el servidor y el `IBrowserFile` solo existe en el cliente. La validación de tamaño se maneja mejor en el UI (como ya se hace con `tamanoExcedido`). Se puede mantener el atributo como referencia pero documentar que la validación real ocurre en el componente.

---

### 2.3 Eliminar archivos huérfanos

**Problema:** Al editar una tarea cambiando la imagen, o eliminar una tarea, el archivo anterior queda en `wwwroot/uploads/`.

**Solución:** Agregar método en un servicio helper o en el repository:

```csharp
// En un nuevo servicio FileService o dentro del repository
public static void EliminarArchivo(string rutaRelativa)
{
    if (string.IsNullOrEmpty(rutaRelativa)) return;
    
    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", rutaRelativa.TrimStart('/'));
    if (File.Exists(fullPath))
    {
        File.Delete(fullPath);
    }
}
```

**Modificar EditarTarea.razor - ValidSubmit:**
- Antes de asignar nueva imagen, si `tareaModel.Imagen` ya tiene valor, eliminar el archivo anterior.

**Modificar TareaState.EliminarTarea:**
- Antes de eliminar del repositorio, si la tarea tiene imagen, eliminar el archivo.

---

### 2.4 Breadcrum: usar TareaState en lugar de ITareaRepository

**Archivo:** `Components/Shared/Breadcrum.razor`

**Problema:** Inyecta `ITareaRepository` directo, bypassando el state compartido.

**Solución:** Cambiar inyección a `TareaState` y usar `GetTareaById`.

---

### 2.5 Eliminar páginas template sin usar

**Eliminar archivos:**
- `Components/Pages/Counter.razor`
- `Components/Pages/Weather.razor`

**Limpiar NavMenu:** Quitar links a /counter y /weather si existen.

---

## FASE 3: Decisión SQLite

### 3.1 Resolver DbContext registrado pero sin usar

**Archivo:** `Program.cs`

**Opción A (Recomendada):** Cambiar el repository activo a SQLite
```csharp
builder.Services.AddScoped<ITareaRepository, TareaSqliteRepository>();
```

**Opción B:** Eliminar el registro de DbContext si no se va a usar SQLite
```csharp
// Eliminar estas líneas:
// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseSqlite("Data Source=tareas.db"));
```

---

## FASE 4: Documentación y Entrega

### 4.1 README.md

**Contenido:**

```markdown
# TareasBlazor

Aplicación de gestión de tareas (TODO) desarrollada con Blazor Server (.NET 8).

## Características

- CRUD completo de tareas
- Carga de imágenes con vista previa
- Filtros por prioridad y estado
- Dashboard con estadísticas
- Validación de formulario con DataAnnotations
- Almacenamiento con ProtectedLocalStorage

## Requisitos

- .NET 8 SDK
- Un navegador moderno

## Ejecución

```bash
dotnet run
```

La aplicación estará disponible en `https://localhost:7245` o `http://localhost:5017`.

## Estructura del proyecto

```
Components/
  Pages/        # Páginas de la aplicación
  Shared/       # Componentes reutilizables
  Layout/       # Layouts principales
Models/         # Modelos de datos
Infrastructure/ # Repositorios, interfaces, base de datos
Validation/     # Atributos de validación custom
Helpers/        # Helpers estáticos
Shared/         # State management (TareaState)
```

## Repositorios disponibles

| Repository | Almacenamiento |
|---|---|
| TareaProtectedLocalStorageRepository | Server-side encrypted storage (activo) |
| TareaSqliteRepository | SQLite (tareas.db) |
| TareaLocalStorageRepository | Browser localStorage |
| TareaRepository | In-memory |

Para cambiar el almacenamiento, modificar la línea en `Program.cs`:
```csharp
builder.Services.AddScoped<ITareaRepository, TuRepositoryElegido>();
```
```

### 4.2 Verificar .gitignore

**Asegurar que incluya:**
```
uploads/
*.db
bin/
obj/
```

### 4.3 Preparar push a GitHub

```bash
git add .
git commit -m "feat: TareasBlazor - aplicación completa de gestión de tareas"
git remote add origin <url-del-repo>
git push -u origin main
```

---

## Resumen de cambios por archivo

| Archivo | Acción |
|---|---|
| `Shared/ImageUploader.razor` | NUEVO |
| `Shared/TareaForm.razor` | NUEVO |
| `Shared/FilterBar.razor` | NUEVO |
| `Shared/ConfirmDialog.razor` | NUEVO |
| `Pages/NuevaTarea.razor` | MODIFICAR (usar ImageUploader + TareaForm) |
| `Pages/EditarTarea.razor` | MODIFICAR (usar ImageUploader + TareaForm) |
| `Shared/ListaTareas.razor` | MODIFICAR (usar FilterBar + ConfirmDialog, fix doble suscripción) |
| `Shared/Breadcrum.razor` | MODIFICAR (usar TareaState) |
| `Validation/ArchivoValidationAttribute.cs` | MODIFICAR (implementar PesoArchivoAttribute) |
| `TareaState.cs` | MODIFICAR (eliminar archivos huérfanos en EliminarTarea) |
| `Program.cs` | MODIFICAR (decidir SQLite) |
| `Pages/Counter.razor` | ELIMINAR |
| `Pages/Weather.razor` | ELIMINAR |
| `README.md` | NUEVO |
| `.gitignore` | VERIFICAR |
