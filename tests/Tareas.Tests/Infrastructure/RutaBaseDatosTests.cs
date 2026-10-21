using FluentAssertions;

using Tareas.Infrastructure.Persistence;

using Xunit;

namespace Tareas.Tests.Infrastructure;

public class RutaBaseDatosTests : IDisposable
{
    private readonly string _raiz;

    public RutaBaseDatosTests()
    {
        _raiz = Path.Combine(Path.GetTempPath(), $"pw-ruta-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_raiz);
    }

    public void Dispose()
    {
        if (Directory.Exists(_raiz))
        {
            Directory.Delete(_raiz, true);
        }
    }

    [Fact]
    public void ResolverRuta_UsaElDirectorioIndicadoCuandoAdmiteEscritura()
    {
        var dataDir = Path.Combine(_raiz, "data");

        var ruta = RutaBaseDatos.ResolverRuta(dataDir);

        ruta.Should().Be(Path.Combine(dataDir, RutaBaseDatos.NombreArchivo));
        Directory.Exists(dataDir).Should().BeTrue();
    }

    [Fact]
    public void ResolverRuta_CaeAlRespaldoCuandoElDirectorioNoAdmiteEscritura()
    {
        var bloqueo = Path.Combine(_raiz, "bloqueado");
        File.WriteAllText(bloqueo, "esto es un archivo, no una carpeta");
        var dataDir = Path.Combine(bloqueo, "data");

        var ruta = RutaBaseDatos.ResolverRuta(dataDir);

        ruta.Should().Be(Path.Combine(Path.GetTempPath(), RutaBaseDatos.CarpetaRespaldo, RutaBaseDatos.NombreArchivo));
        Directory.Exists(Path.GetDirectoryName(ruta)).Should().BeTrue();
    }

    [Fact]
    public void ResolverRuta_UsaLaCarpetaLocalCuandoNoSeIndicaDataDir()
    {
        var ruta = RutaBaseDatos.ResolverRuta(null);

        ruta.Should().EndWith(RutaBaseDatos.NombreArchivo);
        ruta.Should().StartWith(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            because: "sin DATA_DIR la base queda en la carpeta local del usuario");
    }

    [Fact]
    public void ResolverConexion_UsaLaCadenaDeConfiguracionSiEstaDefinida()
    {
        RutaBaseDatos.ResolverConexion("Data Source=/tmp/custom.db", "/tmp/data")
            .Should().Be("Data Source=/tmp/custom.db");
    }

    [Fact]
    public void ResolverConexion_ConstruyeLaCadenaConDataDirCuandoNoHayCadena()
    {
        var dataDir = Path.Combine(_raiz, "data");

        var conexion = RutaBaseDatos.ResolverConexion(null, dataDir);

        conexion.Should().Be($"Data Source={Path.Combine(dataDir, RutaBaseDatos.NombreArchivo)}");
    }
}
