# TareasBlazor

Aplicación de gestión de tareas (TODO) desarrollada con Blazor Server (.NET 8).

## Características

- CRUD completo de tareas
- Carga de imágenes con vista previa en base64
- Filtros por prioridad (Baja, Media, Alta) y estado (Completadas, Pendientes)
- Dashboard con estadísticas en tiempo real
- Validación de formulario con DataAnnotations y atributos personalizados
- Componentes reutilizables: `TareaForm`, `ImageUploader`, `FilterBar`, `ConfirmDialog`
- Almacenamiento con ProtectedLocalStorage (encriptado en servidor)

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
  Pages/        # Páginas de la aplicación (Home, NuevaTarea, EditarTarea, ListaTareas)
  Shared/       # Componentes reutilizables (TareaForm, ImageUploader, FilterBar, ConfirmDialog, etc.)
  Layout/       # Layouts principales (MainLayout, NavMenu)
Models/         # Modelos de datos (TareaModel)
Infrastructure/ # Repositorios, interfaces, base de datos
Validation/     # Atributos de validación personalizados (FutureDate, ArchivoImagen)
Helpers/        # Helpers estáticos (PrioridadHelper)
Shared/         # State management (TareaState)
```

## Componentes reutilizables

| Componente | Descripción |
|---|---|
| `TareaForm` | Formulario completo de tarea con validación, acepta `ChildContent` para extender |
| `ImageUploader` | Carga de imagen con preview, validación de tamaño, botón limpiar |
| `FilterBar` | Filtros de prioridad y estado con contador de resultados |
| `ConfirmDialog` | Modal de confirmación genérico configurable |
| `StatsCard` | Tarjeta de estadísticas con título, valor y color |
| `TareaItem` | Item de lista con checkbox, badges y botones de acción |
| `Breadcrum` | Navegación breadcrumb dinámica |

## Repositorios disponibles

| Repository | Almacenamiento |
|---|---|
| `TareaProtectedLocalStorageRepository` | Server-side encrypted storage **(activo)** |
| `TareaSqliteRepository` | SQLite (`tareas.db`) |
| `TareaLocalStorageRepository` | Browser localStorage (vía JS interop) |
| `TareaRepository` | In-memory (lista estática) |

Para cambiar el almacenamiento, modificar la línea en `Program.cs`:

```csharp
builder.Services.AddScoped<ITareaRepository, TuRepositoryElegido>();
```

## Validaciones

| Campo | Reglas |
|---|---|
| `Titulo` | Requerido, 3-50 caracteres |
| `Descripcion` | Requerida, máx. 200 caracteres |
| `FechaVencimiento` | Requerida, debe ser fecha futura |
| `Prioridad` | Requerida, valor válido del enum |
| `Imagen` | Extensiones válidas: .jpg, .jpeg, .png, .gif, .webp |
| Regla de negocio | Tareas de alta prioridad deben vencer en ≤ 7 días |
