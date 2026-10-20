using Tareas.Application.Abstractions;
using Tareas.Application.Configuracion;
using Tareas.Application.Validation;

namespace Tareas.Infrastructure.Almacenamiento;

/// <summary>
/// Escritura y borrado de archivos adjuntos en disco. Centraliza la I/O que antes estaba
/// duplicada en dos páginas y en el estado de presentación, incluida la validación de
/// contención de ruta sobre valores provenientes de la base de datos.
/// </summary>
public sealed class ArchivoStorage : IArchivoStorage
{
    private readonly string _webRootPath;
    private readonly OpcionesAlmacenamiento _opciones;

    public ArchivoStorage(string webRootPath, OpcionesAlmacenamiento opciones)
    {
        _webRootPath = webRootPath;
        _opciones = opciones;
    }

    public async Task<string> GuardarAsync(Stream contenido, string extension, CancellationToken ct = default)
    {
        var ext = (extension ?? string.Empty).ToLowerInvariant();
        if (!ArchivoImagenAttribute.ExtensionesPermitidas.Contains(ext))
        {
            throw new ArgumentException($"Extensión de imagen no permitida: {ext}", nameof(extension));
        }

        var raizUploads = Path.GetFullPath(Path.Combine(_webRootPath, _opciones.RutaUploads));
        Directory.CreateDirectory(raizUploads);

        var nombre = Path.GetRandomFileName() + ext;
        var destino = Path.Combine(raizUploads, nombre);

        try
        {
            await using (var archivo = new FileStream(destino, FileMode.CreateNew))
            {
                var buffer = new byte[81920];
                long total = 0;
                int leidos;

                while ((leidos = await contenido.ReadAsync(buffer, ct)) > 0)
                {
                    total += leidos;
                    if (total > _opciones.MaximoBytes)
                    {
                        throw new InvalidOperationException(
                            $"El archivo supera el tamaño máximo de {_opciones.MaximoBytes} bytes.");
                    }

                    await archivo.WriteAsync(buffer.AsMemory(0, leidos), ct);
                }
            }
        }
        catch
        {
            if (File.Exists(destino))
            {
                File.Delete(destino);
            }

            throw;
        }

        return $"/{_opciones.RutaUploads.Trim('/')}/{nombre}";
    }

    public Task EliminarAsync(string rutaRelativa, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(rutaRelativa))
        {
            return Task.CompletedTask;
        }

        var completa = Path.GetFullPath(Path.Combine(
            _webRootPath, rutaRelativa.Replace('\\', '/').TrimStart('/')));

        var raizUploads = Path.GetFullPath(Path.Combine(_webRootPath, _opciones.RutaUploads));
        var prefijo = raizUploads.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!completa.StartsWith(prefijo, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"La ruta '{rutaRelativa}' queda fuera del directorio de uploads.");
        }

        if (File.Exists(completa))
        {
            File.Delete(completa);
        }

        return Task.CompletedTask;
    }
}
