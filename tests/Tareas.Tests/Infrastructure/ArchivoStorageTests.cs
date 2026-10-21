using FluentAssertions;

using Tareas.Application.Configuracion;
using Tareas.Infrastructure.Almacenamiento;

using Xunit;

namespace Tareas.Tests.Infrastructure;

public class ArchivoStorageTests : IDisposable
{
    private readonly string _raiz;

    public ArchivoStorageTests()
    {
        _raiz = Path.Combine(Path.GetTempPath(), $"pw-almacen-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_raiz);
    }

    public void Dispose()
    {
        if (Directory.Exists(_raiz))
        {
            Directory.Delete(_raiz, true);
        }
    }

    private static Stream Contenido() => new MemoryStream(new byte[] { 1, 2, 3, 4 });

    [Fact]
    public async Task GuardarAsync_EscribeEnElWwwRootCuandoNoHayRutaFisica()
    {
        var webRoot = Path.Combine(_raiz, "wwwroot");
        var storage = new ArchivoStorage(webRoot, new OpcionesAlmacenamiento());

        var ruta = await storage.GuardarAsync(Contenido(), ".png");

        ruta.Should().StartWith("/uploads/");
        Directory.GetFiles(Path.Combine(webRoot, "uploads")).Should().ContainSingle();
    }

    [Fact]
    public async Task GuardarAsync_EscribeEnLaRutaFisicaYDevuelveLaUrlPublica()
    {
        var webRoot = Path.Combine(_raiz, "wwwroot");
        var fisico = Path.Combine(_raiz, "adjuntos");
        var storage = new ArchivoStorage(webRoot, new OpcionesAlmacenamiento
        {
            RutaFisicaUploads = fisico
        });

        var ruta = await storage.GuardarAsync(Contenido(), ".png");

        ruta.Should().StartWith("/uploads/");
        Directory.GetFiles(fisico).Should().ContainSingle();
        Directory.Exists(Path.Combine(webRoot, "uploads")).Should().BeFalse();
    }

    [Fact]
    public async Task GuardarAsync_RechazaExtensionesNoPermitidas()
    {
        var storage = new ArchivoStorage(Path.Combine(_raiz, "wwwroot"), new OpcionesAlmacenamiento());

        var act = async () => await storage.GuardarAsync(Contenido(), ".exe");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task EliminarAsync_BorraElArchivoFisicoDeLaRutaPublica()
    {
        var fisico = Path.Combine(_raiz, "adjuntos");
        var storage = new ArchivoStorage(Path.Combine(_raiz, "wwwroot"), new OpcionesAlmacenamiento
        {
            RutaFisicaUploads = fisico
        });

        var ruta = await storage.GuardarAsync(Contenido(), ".png");
        var archivo = Directory.GetFiles(fisico).Single();

        await storage.EliminarAsync(ruta);

        File.Exists(archivo).Should().BeFalse();
    }

    [Fact]
    public async Task EliminarAsync_NoFallaSiElArchivoNoExiste()
    {
        var storage = new ArchivoStorage(Path.Combine(_raiz, "wwwroot"), new OpcionesAlmacenamiento());

        var act = async () => await storage.EliminarAsync("/uploads/no-existe.png");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EliminarAsync_RechazaRutasFueraDelDirectorioDeUploads()
    {
        var storage = new ArchivoStorage(Path.Combine(_raiz, "wwwroot"), new OpcionesAlmacenamiento());

        var act = async () => await storage.EliminarAsync("/uploads/../../passwd");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
