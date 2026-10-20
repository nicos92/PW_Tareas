using Tareas.Application.Dtos;
using Tareas.Domain.Entidades;

namespace Tareas.Application.Abstractions;

/// <summary>
/// Puerto de persistencia de tareas. Único punto de acceso a la entidad
/// <see cref="Entidades.Tarea"/>; la capa de presentación nunca lo ve directamente.
/// </summary>
public interface ITareaRepository
{
    Task<PaginadoResult<Tarea>> ObtenerPaginadoAsync(
        ParametrosPaginacion paginacion,
        FiltroTareas filtro,
        CancellationToken ct = default);

    Task<Tarea?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    Task AgregarAsync(Tarea tarea, CancellationToken ct = default);

    Task ActualizarAsync(Tarea tarea, CancellationToken ct = default);

    Task EliminarAsync(Guid id, CancellationToken ct = default);

    Task CambiarCompletadaAsync(Guid id, bool completada, CancellationToken ct = default);

    Task<EstadisticasTareas> ObtenerEstadisticasAsync(CancellationToken ct = default);

    Task EliminarTodasAsync(CancellationToken ct = default);
}
