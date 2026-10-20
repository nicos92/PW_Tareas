using Tareas.Application.Abstractions;

namespace Tareas.Application.CasosDeUso.Comandos;

/// <summary>
/// Elimina la tarea y, si la tiene, su archivo adjunto. La entidad se borra primero:
/// si la limpieza del archivo falla queda un archivo huérfano invisible en vez de una
/// tarea apuntando a una imagen inexistente. Absorbe la orquestación de
/// <c>TareaState.EliminarTarea</c>.
/// </summary>
public sealed class EliminarTareaCommand(
    ITareaRepository repositorio,
    IArchivoStorage almacenamiento)
{
    public async Task EjecutarAsync(Guid id, CancellationToken ct = default)
    {
        var tarea = await repositorio.ObtenerPorIdAsync(id, ct);
        if (tarea is null)
        {
            return;
        }

        await repositorio.EliminarAsync(id, ct);

        if (!string.IsNullOrEmpty(tarea.Imagen))
        {
            await almacenamiento.EliminarAsync(tarea.Imagen, ct);
        }
    }
}
