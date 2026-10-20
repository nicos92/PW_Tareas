using Tareas.Application.Abstractions;
using Tareas.Application.Dtos;
using Tareas.Domain.Entidades;

namespace Tareas.Application.CasosDeUso.Comandos;

/// <summary>
/// Valida y actualiza una tarea existente. Si llega una imagen nueva la guarda primero,
/// persiste y recién entonces borra la anterior, de modo que un fallo nunca deja la tarea
/// apuntando a un archivo inexistente. Si no llega imagen nueva, respeta el valor de
/// <c>dto.Imagen</c> (permite quitar la imagen existente).
/// </summary>
public sealed class ActualizarTareaCommand(
    ITareaRepository repositorio,
    IArchivoStorage almacenamiento,
    ISystemClock reloj)
{
    public async Task EjecutarAsync(
        Guid id,
        TareaDto dto,
        SubidaArchivo? imagen,
        CancellationToken ct = default)
    {
        var tarea = await repositorio.ObtenerPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No existe la tarea {id}.");

        var imagenAnterior = tarea.Imagen;

        // Sin imagen nueva se conserva la ruta que trae el dto (permite limpiarla).
        var imagenFinal = imagen is null ? (dto.Imagen ?? string.Empty) : imagenAnterior;

        // Valida las invariantes ANTES de escribir nada en disco.
        tarea.Actualizar(
            dto.Titulo, dto.Descripcion, dto.FechaVencimiento,
            dto.Prioridad, dto.Categoria, imagenFinal, dto.Completada, reloj.Hoy);

        if (imagen is not null)
        {
            await using var contenido = imagen.Contenido;
            tarea.AsignarImagen(await almacenamiento.GuardarAsync(contenido, imagen.Extension, ct));
        }

        await repositorio.ActualizarAsync(tarea, ct);

        // Limpia el archivo anterior cuando ya no está referenciado por la tarea.
        var imagenNueva = tarea.Imagen;
        if (!string.IsNullOrEmpty(imagenAnterior) && !string.Equals(imagenAnterior, imagenNueva, StringComparison.Ordinal))
        {
            await almacenamiento.EliminarAsync(imagenAnterior, ct);
        }
    }
}
