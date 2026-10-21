namespace Tareas.Application.Configuracion;

/// <summary>
/// Opciones de almacenamiento de archivos adjuntos. Vive en Application para que tanto la
/// UI (límite de subida) como Infrastructure (escritura en disco) dependan de la misma fuente,
/// en lugar del literal <c>1024 * 1024</c> repetido en cuatro lugares.
/// </summary>
public sealed class OpcionesAlmacenamiento
{
    public const string RutaUploadsPorDefecto = "uploads";
    public const long MaximoBytesPorDefecto = 1024 * 1024;

    /// <summary>
    /// Carpeta pública de los adjuntos, relativa al <c>wwwroot</c>. Es la parte de la URL
    /// que se devuelve al cliente (p. ej. <c>/uploads/foto.png</c>).
    /// </summary>
    public string RutaUploads { get; set; } = RutaUploadsPorDefecto;

    /// <summary>
    /// Ruta absoluta en disco donde se escriben los adjuntos. Si es <c>null</c> se usan
    /// <see cref="RutaUploads"/> dentro del <c>wwwroot</c>. Permite separar la carpeta
    /// escribible del contenedor (Cloud Run) de la carpeta pública que sirve el sitio.
    /// </summary>
    public string? RutaFisicaUploads { get; set; }

    public long MaximoBytes { get; set; } = MaximoBytesPorDefecto;
}
