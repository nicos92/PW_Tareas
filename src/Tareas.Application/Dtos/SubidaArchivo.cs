namespace Tareas.Application.Dtos;

/// <summary>
/// Contenido a subir a disco, ya abierto por la capa de presentación.
/// Existe para que Application no dependa de <c>IBrowserFile</c> ni de Blazor.
/// </summary>
public sealed record SubidaArchivo(Stream Contenido, string NombreOriginal)
{
    public string Extension => Path.GetExtension(NombreOriginal);
}
