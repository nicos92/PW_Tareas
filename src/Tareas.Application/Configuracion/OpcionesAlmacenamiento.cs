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

    public string RutaUploads { get; set; } = RutaUploadsPorDefecto;

    public long MaximoBytes { get; set; } = MaximoBytesPorDefecto;
}
