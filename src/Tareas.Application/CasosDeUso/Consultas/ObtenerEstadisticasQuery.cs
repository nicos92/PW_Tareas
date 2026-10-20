using Tareas.Application.Abstractions;
using Tareas.Application.Dtos;

namespace Tareas.Application.CasosDeUso.Consultas;

/// <summary>
/// Agregados de tablero. Se ejecuta en paralelo con <see cref="ObtenerTareasQuery"/>.
/// </summary>
public sealed class ObtenerEstadisticasQuery(ITareaRepository repositorio)
{
    public Task<EstadisticasTareas> EjecutarAsync(CancellationToken ct = default)
        => repositorio.ObtenerEstadisticasAsync(ct);
}
