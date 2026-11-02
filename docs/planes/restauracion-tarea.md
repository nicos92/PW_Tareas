# Plan de implementación: filtro de tareas dadas de baja y restauración

## Decisiones

- Se agrega el enum `EstadoBajaFiltro { Activas, DadasDeBaja, Todas }` en la capa de Aplicación.
- El valor por defecto es `Activas`, por lo que el listado mantiene el comportamiento actual.
- El filtro se incorpora a `FiltroTareas`; `ObtenerTareasQuery` conserva su firma.
- Las estadísticas siguen calculándose sólo sobre tareas activas.
- La restauración se ejecuta directamente desde la tabla, sin diálogo de confirmación.
- No se requiere una migración nueva: la columna `IsDeleted` ya existe.

## Dominio

Archivo: `src/Tareas.Domain/Entidades/Tarea.cs`

- Agregar `public void Restaurar()` con comportamiento idempotente.
- `Restaurar()` cambia `IsDeleted` a `false` sólo si la tarea estaba dada de baja.

## Aplicación

Archivos:

- `src/Tareas.Application/Filtros/EstadoBajaFiltro.cs` (nuevo)
- `src/Tareas.Application/Dtos/FiltroTareas.cs`
- `src/Tareas.Application/Abstractions/ITareaRepository.cs`
- `src/Tareas.Application/CasosDeUso/Comandos/RestaurarTareaCommand.cs` (nuevo)
- `src/Tareas.Application/DependencyInjection.cs`
- `src/Tareas.Application/Dtos/TareaDto.cs`

Cambios:

- Crear el enum `EstadoBajaFiltro`.
- Agregar `EstadoBajaFiltro EstadoBaja = EstadoBajaFiltro.Activas` a `FiltroTareas`.
- Agregar `RestaurarAsync(Guid id)` al puerto `ITareaRepository`.
- Crear `RestaurarTareaCommand`, que delega en el repositorio.
- Registrar `RestaurarTareaCommand` en `AddApplication()`.
- Exponer `IsDeleted` en `TareaDto` y mapearlo desde el dominio.

## Infraestructura

Archivo: `src/Tareas.Infrastructure/Repositories/TareaRepository.cs`

- En `ObtenerPaginadoAsync`, aplicar `IgnoreQueryFilters()` sólo cuando
  `filtro.EstadoBaja` sea distinto de `Activas`.
- Agregar el filtro `IsDeleted` en `ExtensionesQueryableTarea.AplicarFiltros`.
- Implementar `RestaurarAsync` con `IgnoreQueryFilters()`, llamando a
  `Tarea.Restaurar()` y guardando cambios.

No se modifica `TareaDbContext` ni `TareaEntityTypeConfiguration`: el filtro global
`HasQueryFilter(t => !t.IsDeleted)` se conserva para el resto de las consultas.

## Presentación Blazor

Archivos:

- `src/Tareas.Web/Components/Features/Tareas/TareaEstado.cs`
- `src/Tareas.Web/Components/Shared/FilterBar.razor`
- `src/Tareas.Web/Components/Pages/ListaTareas.razor`
- `src/Tareas.Web/Components/Pages/ListaTareas.razor.cs`
- `src/Tareas.Web/Components/Shared/TareaItem.razor`

Cambios:

- Inyectar `RestaurarTareaCommand` en `TareaEstado`.
- Agregar `FiltroBajaTexto`, `SetFiltroBajaAsync` y `RestaurarTareaAsync`.
- Centralizar el ajuste de página al eliminar o restaurar el último elemento.
- Agregar un selector "Estado de baja" con las opciones Activas, Dadas de baja y Todas.
- Mostrar la acción de restaurar sólo cuando `TareaDto.IsDeleted` sea `true`.
- Marcar visualmente la fila dada de baja y deshabilitar el checkbox de completada.

## Tests

Archivos:

- `tests/Tareas.Tests/Domain/TareaBajaTests.cs`
- `tests/Tareas.Tests/Application/Fakes.cs`
- `tests/Tareas.Tests/Application/CasosDeUsoTests.cs`
- `tests/Tareas.Tests/Integration/TareaRepositoryBajaTests.cs`

Cobertura:

- `Restaurar()` reactiva la tarea y es idempotente.
- El comando de restauración devuelve la tarea al listado activo.
- El repositorio filtra correctamente por `DadasDeBaja` y `Todas`.
- El repositorio restaura una tarea que el filtro global ocultaba.

## Verificación

```bash
dotnet build PW_Tareas.sln
dotnet test PW_Tareas.sln
```

Comprobación funcional: dar de baja una tarea, seleccionar "Dadas de baja",
restaurarla y verificar que reaparece en el filtro "Activas".
