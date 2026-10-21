namespace Tareas.Infrastructure.Persistence;

/// <summary>
/// Resolución única de la ruta de la base de datos. Prioriza la cadena de conexión de
/// configuración, luego la variable de entorno <c>DATA_DIR</c> (contenedor Docker) y por
/// defecto la carpeta Local del usuario. Reemplaza el path de identidad de desarrollador
/// que estaba hardcodeado en el composition root.
/// <para>
/// Si el directorio resuelto no admite escritura (por ejemplo <c>/app/data</c> en Cloud Run,
/// donde el sistema de archivos del contenedor es efímero y de sólo lectura), se recurre a una
/// carpeta temporal. Así la aplicación inicia y deja un aviso en el log en lugar de fallar al
/// aplicar las migraciones.
/// </para>
/// </summary>
public static class RutaBaseDatos
{
    public const string NombreArchivo = "tareas.db";
    public const string CarpetaRespaldo = "PW_Tareas";

    public static string ResolverConexion(string? cadenaConexion, string? dataDir)
    {
        if (!string.IsNullOrWhiteSpace(cadenaConexion))
        {
            return cadenaConexion.Contains('=') ? cadenaConexion : $"Data Source={cadenaConexion}";
        }

        return $"Data Source={ResolverRuta(dataDir)}";
    }

    public static string ResolverRuta(string? dataDir)
    {
        var directorio = !string.IsNullOrWhiteSpace(dataDir)
            ? dataDir
            : Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                CarpetaRespaldo);

        return Path.Combine(PrepararDirectorio(directorio), NombreArchivo);
    }

    /// <summary>
    /// Devuelve el directorio en el que se puede escribir, creando la carpeta de respaldo
    /// cuando la ruta preferida no admite escritura.
    /// </summary>
    public static string PrepararDirectorio(string directorio)
    {
        if (EsEscribible(directorio))
        {
            return directorio;
        }

        var respaldo = Path.Combine(Path.GetTempPath(), CarpetaRespaldo);
        Directory.CreateDirectory(respaldo);
        return respaldo;
    }

    /// <summary>
    /// Indica si el directorio existe (o puede crearse) y admite escritura. Se prueba
    /// creando un archivo temporal porque los permisos del directorio pueden no reflejar
    /// el permiso real de escritura.
    /// </summary>
    public static bool EsEscribible(string directorio)
    {
        try
        {
            Directory.CreateDirectory(directorio);

            var prueba = Path.Combine(directorio, $".pw-{Guid.NewGuid():N}.tmp");
            using (File.Create(prueba))
            {
            }

            File.Delete(prueba);
            return true;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            return false;
        }
    }
}
