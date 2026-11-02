using Tareas.Application.Abstractions;

namespace Tareas.Application.CasosDeUso.Comandos;

/// <summary>
/// Da de baja lógica una tarea. Conserva la imagen adjunta para no romper
/// una eventual restauración o auditoría.
/// </summary>
public sealed class DarDeBajaTareaCommand(ITareaRepository repositorio)
{
    public Task EjecutarAsync(Guid id, CancellationToken ct = default)
        => repositorio.DarDeBajaAsync(id, ct);
}
