using Tareas.Application.Abstractions;

namespace Tareas.Application.CasosDeUso.Comandos;

public sealed class RestaurarTareaCommand(ITareaRepository repositorio)
{
    public Task EjecutarAsync(Guid id, CancellationToken ct = default)
        => repositorio.RestaurarAsync(id, ct);
}
