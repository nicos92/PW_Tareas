namespace Tareas.Application.Abstractions;

/// <summary>
/// Puerto de almacenamiento de archivos adjuntos. Centraliza el I/O que antes estaba
/// duplicado en dos componentes de la UI y en el estado de presentación.
/// </summary>
public interface IArchivoStorage
{
    /// <summary>
    /// Escribe el contenido en disco y devuelve la ruta relativa servible
    /// (p. ej. <c>/uploads/abc123.png</c>). No libera el stream: el llamador es responsable.
    /// </summary>
    Task<string> GuardarAsync(Stream contenido, string extension, CancellationToken ct = default);

    /// <summary>
    /// Elimina un archivo por su ruta relativa. Idempotente: no falla si el archivo no existe.
    /// </summary>
    Task EliminarAsync(string rutaRelativa, CancellationToken ct = default);
}
