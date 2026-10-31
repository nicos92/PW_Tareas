# Plan de implementación: baja lógica de tareas

## Decisiones

- La entidad `Tarea` incorpora `bool IsDeleted` con setter privado.
- La baja se realiza con `Tarea.DarDeBaja()`; no se elimina la fila.
- La imagen adjunta se conserva. `DarDeBajaTareaCommand` no depende de `IArchivoStorage`.
- Las APIs internas de `Eliminar` se renombran a `DarDeBaja` para reflejar el dominio.
- Las consultas excluyen las tareas dadas de baja mediante un filtro global de EF Core.

## Dominio

Archivo: `src/Tareas.Domain/Entidades/Tarea.cs`

- Agregar `public bool IsDeleted { get; private set; }`.
- Agregar `public void DarDeBaja()` con comportamiento idempotente.

## Aplicación

Archivos:

- `src/Tareas.Application/Abstractions/ITareaRepository.cs`
- `src/Tareas.Application/CasosDeUso/Comandos/EliminarTareaCommand.cs`
- `src/Tareas.Application/DependencyInjection.cs`

Cambios:

- `EliminarAsync(Guid id)` pasa a `DarDeBajaAsync(Guid id)`.
- Se elimina `EliminarTodasAsync` por no tener consumidores y ser una vía de hard delete.
- `EliminarTareaCommand` pasa a `DarDeBajaTareaCommand`, sin `IArchivoStorage`.
- Actualizar el registro en `AddApplication()`.
- Las queries no cambian: el filtro global de EF Core se encarga de excluir las bajas.

## Infraestructura

Archivos:

- `src/Tareas.Infrastructure/Persistence/Configuraciones/TareaEntityTypeConfiguration.cs`
- `src/Tareas.Infrastructure/Repositories/TareaRepository.cs`
- `src/Tareas.Infrastructure/Persistence/Migrations/*`

Cambios:

- Mapear `IsDeleted` como requerido con valor por defecto `false`.
- Agregar `entity.HasQueryFilter(t => !t.IsDeleted)`.
- Implementar `DarDeBajaAsync` cargando la tarea activa, llamando a `DarDeBaja()` y guardando.
- Eliminar `EliminarTodasAsync`.
- Crear la migración `AgregaBajaLogicaTarea` y aplicarla.

## Presentación Blazor

Archivos:

- `src/Tareas.Web/Components/Features/Tareas/TareaEstado.cs`
- `src/Tareas.Web/Components/Pages/ListaTareas.razor`
- `src/Tareas.Web/Components/Pages/ListaTareas.razor.cs`
- `src/Tareas.Web/Components/Shared/TareaItem.razor`

Cambios:

- Renombrar el estado y el comando a `DarDeBaja`.
- El diálogo de confirmación informa que se dará de baja la tarea.
- El botón de papelera conserva el icono, pero su etiqueta accesible pasa a “Dar de baja la tarea”.
- El listado sigue usando `ResultadoPaginado`, por lo que las tareas dadas de baja desaparecen automáticamente.

## Tests

Archivos:

- `tests/Tareas.Tests/Application/Fakes.cs`
- `tests/Tareas.Tests/Application/CasosDeUsoTests.cs`
- `tests/Tareas.Tests/Domain/TareaBajaTests.cs` (nuevo)
- `tests/Tareas.Tests/Integration/TareaRepositoryBajaTests.cs` (nuevo)

Cobertura:

- `DarDeBaja()` marca `IsDeleted` y es idempotente.
- El comando conserva la imagen y la tarea deja de ser visible.
- El repositorio excluye la tarea de listado, lookup y estadísticas.
- La fila sigue existiendo en la base con `IsDeleted = 1`.

## Documentación

- `docs/ARIA_ATTRIBUTES.md`: actualizar la etiqueta accesible del botón.
- `docs/COMPONENTES_REUTILIZABLES.md`: actualizar el parámetro de `TareaItem`.
- `README.md`: mencionar la baja lógica en las funcionalidades.

## Verificación

```bash
dotnet build
dotnet test
dotnet ef migrations list --project src/Tareas.Infrastructure --context TareaDbContext
```

Comprobación funcional: crear una tarea con imagen, darla de baja y verificar que desaparece de la UI pero la fila y el archivo permanecen.
