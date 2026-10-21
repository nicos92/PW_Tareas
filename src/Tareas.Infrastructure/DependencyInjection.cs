using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Tareas.Application.Abstractions;
using Tareas.Application.Configuracion;
using Tareas.Infrastructure.Almacenamiento;
using Tareas.Infrastructure.Persistence;
using Tareas.Infrastructure.Repositories;
using Tareas.Infrastructure.Time;

namespace Tareas.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra la persistencia, el almacenamiento de archivos y el reloj.
    /// <c>webRootPath</c> se recibe como parámetro (en lugar de inyectar
    /// <c>IWebHostEnvironment</c>) para que este proyecto no dependa de ASP.NET Core.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string webRootPath)
    {
        var cadena = RutaBaseDatos.ResolverConexion(
            configuration.GetConnectionString("Tareas"),
            Environment.GetEnvironmentVariable("DATA_DIR"));

        services.AddDbContext<TareaDbContext>(options => options.UseSqlite(cadena));

        var rutaUploads = configuration["Storage:UploadsPath"];
        var rutaFisicaUploads = configuration["Storage:UploadsPhysicalPath"];
        var opcionesAlmacenamiento = new OpcionesAlmacenamiento
        {
            RutaUploads = string.IsNullOrWhiteSpace(rutaUploads)
                ? OpcionesAlmacenamiento.RutaUploadsPorDefecto
                : rutaUploads,
            RutaFisicaUploads = string.IsNullOrWhiteSpace(rutaFisicaUploads)
                ? null
                : rutaFisicaUploads,
            MaximoBytes = long.TryParse(configuration["Storage:MaxFileSizeBytes"], out var maximo) && maximo > 0
                ? maximo
                : OpcionesAlmacenamiento.MaximoBytesPorDefecto
        };

        services.AddSingleton(opcionesAlmacenamiento);
        services.AddSingleton<IArchivoStorage>(
            _ => new ArchivoStorage(webRootPath, opcionesAlmacenamiento));

        services.AddScoped<ISystemClock, SystemClock>();
        services.AddScoped<ITareaRepository, TareaRepository>();

        return services;
    }
}
