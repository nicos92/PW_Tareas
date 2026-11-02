---
title: PW Tareas
emoji: ✅
colorFrom: blue
colorTo: indigo
sdk: docker
app_port: 8080
pinned: false
license: gpl-3.0
---

# TareasBlazor

[![CI](https://github.com/nicos92/PW_Tareas/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/nicos92/PW_Tareas/actions/workflows/ci.yml)
[![License: GPL-3.0](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE)

Aplicación de gestión de tareas (TODO) desarrollada con Blazor Server (.NET 8) y
organizada con **Clean Architecture** por proyectos.

## Características

- CRUD completo de tareas con baja lógica (las tareas dadas de baja no se muestran)
- Carga de imágenes con vista previa en base64
- Filtros por prioridad (Baja, Media, Alta), estado (Completadas, Pendientes) y búsqueda por título o descripción
- Dashboard con estadísticas en tiempo real
- Validación de formulario con DataAnnotations y atributos personalizados
- Componentes reutilizables: `TareaForm`, `ImageUploader`, `FilterBar`, `ConfirmDialog`
- Persistencia en SQLite vía EF Core con `DbContextFactory` y seed idempotente de categorías

## Requisitos

- .NET 8 SDK
- Un navegador moderno
- Node.js 22 (sólo para el hook de validación de commits; opcional para ejecutar la aplicación)

Para habilitar la validación local de mensajes de commit:

```bash
npm install
```

## Ejecución

```bash
dotnet run --project src/Tareas.Web
```

La aplicación estará disponible en `https://localhost:7245` o `http://localhost:5017`.

## Ejecución con Docker o Podman

Con Docker:

```bash
docker build -t tareas-blazor .
docker run --rm -p 8080:8080 tareas-blazor
```

Con Podman:

```bash
podman build -t tareas-blazor .
podman run --rm -p 8080:8080 tareas-blazor
```

La aplicación estará disponible en `http://localhost:8080`.

Variables de entorno del contenedor:

| Variable | Valor por defecto | Descripción |
|---|---|---|
| `ASPNETCORE_URLS` | `http://+:8080` | Dirección/puerto de escucha |
| `DATA_DIR` | `/app/data` | Carpeta donde se guarda `tareas.db`. Si no admite escritura, la app cae a `/tmp/PW_Tareas` y lo avisa en el log |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | `true` | Procesa `X-Forwarded-Proto`/`X-Forwarded-For` de los proxies (Cloud Run, Hugging Face, Nginx) |
| `Storage__UploadsPhysicalPath` | *(vacío)* | Ruta absoluta donde se guardan los adjuntos. Vacío = `wwwroot/uploads` |

## Despliegue en Hugging Face Spaces

Ver [docs/DESPLIEGUE_HF.md](docs/DESPLIEGUE_HF.md). El README ya incluye el
front matter necesario (`sdk: docker`, `app_port: 8080`), así que basta con
hacer push del repositorio a un Space con SDK Docker.

## Despliegue en Google Cloud (Cloud Run desde Docker Hub)

La imagen se publica en Docker Hub como `docker.io/nicos92/tareas-blazor`
(tags `latest` y `sha-<commit>`). El despliegue se hace desde la consola de
Cloud Run:

1. Crear o editar el servicio `tareas-blazor`.
2. Elegir **Imagen de contenedor** y usar
   `docker.io/nicos92/tareas-blazor:latest` o un tag inmutable
   `docker.io/nicos92/tareas-blazor:sha-<commit>`.
3. Configurar el puerto `8080` y permitir invocaciones sin autenticar.
4. Definir las variables `DATA_DIR=/tmp/data` y
   `Storage__UploadsPhysicalPath=/tmp/uploads`, y limitar `max-instances=1`
   para que todas las peticiones usen la misma base SQLite.

El detalle y la alternativa con Artifact Registry están en
[docs/DESPLIEGUE_GCP.md](docs/DESPLIEGUE_GCP.md).

## Integración y entrega continua

Los workflows viven en `.github/workflows/`:

- `ci.yml`: valida los mensajes de commit, compila la solución y ejecuta las
  pruebas en cada push y pull request.
- `docker-publish.yml`: publica la imagen en Docker Hub al hacer push a `main`
  o al crear un tag `v*`.
- `deploy-cloud-run.yml`: despliegue opcional y manual desde GitHub Actions.

El detalle de secretos, variables y del despliegue manual está en
[docs/CI_CD.md](docs/CI_CD.md).

## Salud del servicio

- `GET /health`: verificación de vida, responde `200` con `{"status":"ok"}`.
- `GET /health/ready`: verifica que SQLite responda; devuelve `503` si no está
  disponible.

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
| `FilterBar` | Filtros de prioridad, estado y búsqueda textual con contador de resultados |
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
→ `%LOCALAPPDATA%/PW_Tareas/tareas.db`. Si el directorio resuelto no admite
escritura (por ejemplo `/app/data` en Cloud Run), se usa `/tmp/PW_Tareas` como
respaldo y la ruta real se informa en el log al arrancar.

Los repositorios reciben `IDbContextFactory<TareaDbContext>` y crean un contexto
por operación, evitando contextos de larga vida en los circuitos de Blazor Server.

Al iniciar se ejecuta el seed idempotente `SeedData.InicializarAsync`, que crea
las categorías por defecto (`Trabajo`, `Personal`, `Estudio`, `Salud`) sólo si no
existen.

Los adjuntos se guardan en `wwwroot/uploads` (`Storage:UploadsPath`) y se sirven
en `/uploads/...`. Con `Storage:UploadsPhysicalPath` se puede escribir en otra
carpeta del disco (por ejemplo `/tmp/uploads` en Cloud Run) sin cambiar la URL
pública.

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

## Licencia

Copyright (C) 2026 nicos92.

Este programa es software libre: podés redistribuirlo y/o modificarlo bajo los
términos de la **GNU General Public License versión 3** publicada por la Free
Software Foundation. Se distribuye con la esperanza de que sea útil, pero
**sin ninguna garantía**, ni siquiera la garantía implícita de comerciabilidad
o aptitud para un propósito particular. Ver [LICENSE](LICENSE) para el texto
completo.
