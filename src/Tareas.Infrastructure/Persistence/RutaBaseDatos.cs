namespace Tareas.Infrastructure.Persistence;

/// <summary>
/// Resolución única de la ruta de la base de datos. Prioriza la cadena de conexión de
/// configuración, luego la variable de entorno <c>DATA_DIR</c> (contenedor Docker) y por
/// defecto la carpeta Local del usuario. Reemplaza el path de identidad de desarrollador
/// que estaba hardcodeado en el composition root.
/// </summary>
public static class RutaBaseDatos
{
    public const string NombreArchivo = "tareas.db";

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
                "PW_Tareas");

        Directory.CreateDirectory(directorio);

        return Path.Combine(directorio, NombreArchivo);
    }
}
