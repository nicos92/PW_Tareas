---
title: PW Tareas
emoji: ✅
colorFrom: blue
colorTo: indigo
sdk: docker
app_port: 8080
pinned: false
---

# TareasBlazor

Aplicación de gestión de tareas (TODO) desarrollada con Blazor Server (.NET 8) y
organizada con **Clean Architecture** por proyectos.

## Características

- CRUD completo de tareas
- Carga de imágenes con vista previa en base64
- Filtros por prioridad (Baja, Media, Alta) y estado (Completadas, Pendientes)
- Dashboard con estadísticas en tiempo real
- Validación de formulario con DataAnnotations y atributos personalizados
- Componentes reutilizables: `TareaForm`, `ImageUploader`, `FilterBar`, `ConfirmDialog`
- Persistencia en SQLite vía EF Core; almacenamiento de imágenes en disco

## Requisitos

- .NET 8 SDK
- Un navegador moderno

## Ejecución

```bash
dotnet run --project src/Tareas.Web
```

La aplicación estará disponible en `https://localhost:7245` o `http://localhost:5017`.

## Ejecución con Docker

```bash
docker build -t tareas-blazor .
docker run --rm -p 8080:8080 tareas-blazor
```

La aplicación estará disponible en `http://localhost:8080`.

Variables de entorno del contenedor:

| Variable | Valor por defecto | Descripción |
|---|---|---|
| `ASPNETCORE_URLS` | `http://+:8080` | Dirección/puerto de escucha |
| `DATA_DIR` | `/app/data` | Carpeta donde se guarda `tareas.db` |

## Despliegue en Hugging Face Spaces

Ver [docs/DESPLIEGUE_HF.md](docs/DESPLIEGUE_HF.md). El README ya incluye el
front matter necesario (`sdk: docker`, `app_port: 8080`), así que basta con
hacer push del repositorio a un Space con SDK Docker.

## Arquitectura

Clean Architecture por proyectos (sin API HTTP). Regla de dependencia:
`Web → Application → Domain` e `Infrastructure → Application`. **Domain no depende de nadie.**

```
PW_Tareas.sln
├── src/
│   ├── Tareas.Domain/         # Entidad Tarea, reglas de negocio, value objects. SIN dependencias.
│   ├── Tareas.Application/    # Casos de uso, DTOs, puertos (interfaces). Sólo depende de Domain.
│   ├── Tareas.Infrastructure/ # EF Core, migraciones, repositorio, almacenamiento, reloj.
│   └── Tareas.Web/            # Front end Blazor. Composition root.
└── tests/
    └── Tareas.Tests/          # xUnit + FluentAssertions (dominio, aplicación, integración).
```

Detalle de la migración en [docs/PLAN_LIMPIA_ARQUITECTURA.md](docs/PLAN_LIMPIA_ARQUITECTURA.md).

### Estructura del front end (`src/Tareas.Web`)

```
Components/
  Pages/         # Páginas ruteables (Home, ListaTareas, Tarea, NuevaTarea, EditarTarea, Configuracion)
  Shared/        # Componentes reutilizables (TareaForm, ImageUploader, FilterBar, TareaCard, TareaItem, ...)
  Layout/        # MainLayout, NavMenu, StatsPanel
  Features/
    Tareas/      # TareaEstado (estado de presentación) y su base de suscripción
    Tema/        # ThemeService
    Archivos/    # Adaptador IBrowserFile -> SubidaArchivo
Helpers/         # PrioridadHelper
```

Los componentes reciben sólo **DTOs** (`TareaDto`); nunca ven entidades de dominio,
`DbContext` ni strings de filtro.

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

## Persistencia

Una única implementación de `ITareaRepository`:

| Implementación | Almacenamiento |
|---|---|
| `Tareas.Infrastructure.Repositories.TareaRepository` | SQLite (`tareas.db`) vía EF Core |

La ruta de la base de datos se resuelve así (en orden):
`ConnectionStrings:Tareas` en `appsettings.json` → variable de entorno `DATA_DIR`
→ `%LOCALAPPDATA%/PW_Tareas/tareas.db`.

### Migraciones EF Core

```bash
dotnet ef migrations add <Nombre> --project src/Tareas.Infrastructure --context TareaDbContext
dotnet ef database update --project src/Tareas.Infrastructure --context TareaDbContext
```

Las migraciones se aplican automáticamente al arrancar la aplicación.

## Publicación

```bash
dotnet publish src/Tareas.Web/Tareas.Web.csproj -c Release -o ./publish
```

Esto genera la aplicación compilada en la carpeta `./publish` lista para desplegar en IIS, Docker, Azure App Services, etc.

## Paquetes NuGet

| Proyecto | Paquete | Versión |
|---|---|---|
| `Tareas.Infrastructure` | `Microsoft.EntityFrameworkCore.Sqlite` | 8.0.26 |
| `Tareas.Infrastructure` | `Microsoft.EntityFrameworkCore.Design` | 8.0.26 |
| `Tareas.Web` | `Microsoft.EntityFrameworkCore.Sqlite` | 8.0.26 |
| `Tareas.Tests` | `FluentAssertions`, `xunit`, `Microsoft.NET.Test.Sdk` | — |

## Tests

```bash
dotnet test
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

Los textos de error provienen de `Tareas.Domain.Reglas.TareaReglas` (fuente única).
