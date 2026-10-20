using Tareas.Application.Abstractions;
using Tareas.Application.Dtos;
using Tareas.Domain.Entidades;

namespace Tareas.Application.CasosDeUso.Comandos;

/// <summary>
/// Valida vía dominio, persiste y genera el <c>IdPublic</c>.
/// Absorbe la orquestación que vivía en <c>NuevaTarea.razor</c>.
/// </summary>
public sealed class CrearTareaCommand(
    ITareaRepository repositorio,
    IArchivoStorage almacenamiento,
    ISystemClock reloj)
{
    public async Task<Guid> EjecutarAsync(
        TareaDto dto,
        SubidaArchivo? imagen,
        CancellationToken ct = default)
    {
        // Validar ANTES de escribir en disco: así un dto inválido no deja archivos huérfanos.
        var tarea = Tarea.Crear(
            dto.Titulo, dto.Descripcion, dto.FechaVencimiento,
            dto.Prioridad, dto.Categoria, imagen: string.Empty, reloj.Hoy);

        if (imagen is not null)
        {
            await using var contenido = imagen.Contenido;
            tarea.AsignarImagen(await almacenamiento.GuardarAsync(contenido, imagen.Extension, ct));
        }

        await repositorio.AgregarAsync(tarea, ct);
        return tarea.IdPublic;
    }
}
