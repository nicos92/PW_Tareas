using Tareas.Application.Abstractions;

namespace Tareas.Application.CasosDeUso.Comandos;

/// <summary>
/// Alterna el estado de completada de una tarea. La resuelve <b>por id</b> en el repositorio
/// y no desde la página cargada en pantalla, lo que corrige el bug latente del
/// <c>Toggle</c> anterior (un toggle sobre una tarea fuera de la página actual no hacía nada).
/// </summary>
public sealed class CambiarCompletadaTareaCommand(ITareaRepository repositorio)
{
    public async Task EjecutarAsync(Guid id, CancellationToken ct = default)
    {
        var tarea = await repositorio.ObtenerPorIdAsync(id, ct);
        if (tarea is null)
        {
            return;
        }

        await repositorio.CambiarCompletadaAsync(id, !tarea.Completada, ct);
    }
}
