using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;

namespace TareasBlazor.Helpers;

/// <summary>
/// Registro de los archivos estáticos del sitio. Además del <c>wwwroot</c>, publica los
/// adjuntos desde la ruta física configurada cuando ésta está fuera del <c>wwwroot</c>
/// (Cloud Run, donde la carpeta escribible es efímera), manteniendo la misma URL pública.
/// </summary>
public static class ArchivosEstaticosExtension
{
    public static IApplicationBuilder UseArchivosEstaticos(
        this IApplicationBuilder app,
        string? rutaFisicaUploads,
        string rutaPublicaUploads)
    {
        app.UseStaticFiles();

        if (string.IsNullOrWhiteSpace(rutaFisicaUploads))
        {
            return app;
        }

        Directory.CreateDirectory(rutaFisicaUploads);

        var opciones = new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(rutaFisicaUploads),
            RequestPath = "/" + rutaPublicaUploads.Trim('/')
        };

        return app.UseStaticFiles(opciones);
    }
}
