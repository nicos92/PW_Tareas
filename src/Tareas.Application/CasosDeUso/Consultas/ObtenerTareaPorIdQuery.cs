using Tareas.Application.Abstractions;
using Tareas.Application.Dtos;

namespace Tareas.Application.CasosDeUso.Consultas;

/// <summary>
/// Lookup de una tarea por su identificador público. Absorbe <c>TareaState.GetTareaById</c>.
/// </summary>
public sealed class ObtenerTareaPorIdQuery(ITareaRepository repositorio)
{
    public async Task<TareaDto?> EjecutarAsync(Guid id, CancellationToken ct = default)
    {
        var tarea = await repositorio.ObtenerPorIdAsync(id, ct);
        return tarea is null ? null : TareaDto.DesdeDominio(tarea);
    }
}
