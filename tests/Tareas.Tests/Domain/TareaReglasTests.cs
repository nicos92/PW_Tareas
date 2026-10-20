using FluentAssertions;

using Tareas.Domain.Entidades;
using Tareas.Domain.Errores;
using Tareas.Domain.Reglas;
using Tareas.Domain.ValueObjects;

using Xunit;

namespace Tareas.Tests.Domain;

public class TareaReglasTests
{
    private static readonly DateOnly Hoy = new(2026, 1, 1);

    [Fact]
    public void Crear_con_datos_validos_construye_la_entidad()
    {
        var tarea = Tarea.Crear("Comprar pan", "Ir a la panadería", Hoy.AddDays(3),
            Prioridad.Media, Categoria.Ninguna, string.Empty, Hoy);

        tarea.IdPublic.Should().NotBe(Guid.Empty);
        tarea.Titulo.Should().Be("Comprar pan");
        tarea.Completada.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Crear_prioridad_alta_acepta_hasta_siete_dias(int dias)
    {
        var acto = () => Tarea.Crear("Tarea", "Descripción válida", Hoy.AddDays(dias),
            Prioridad.Alta, Categoria.Backend, string.Empty, Hoy);

        acto.Should().NotThrow();
    }

    [Fact]
    public void Crear_prioridad_alta_rechaza_mas_de_siete_dias()
    {
        var acto = () => Tarea.Crear("Tarea", "Descripción válida", Hoy.AddDays(8),
            Prioridad.Alta, Categoria.Backend, string.Empty, Hoy);

        acto.Should().Throw<ViolacionReglaException>()
            .Which.Violaciones.Select(v => v.Mensaje)
            .Should().Contain(TareaReglas.PrioridadAltaLimiteSieteDias);
    }

    [Fact]
    public void Crear_rechaza_fecha_no_futura()
    {
        var acto = () => Tarea.Crear("Tarea", "Descripción válida", Hoy.AddDays(-1),
            Prioridad.Media, Categoria.Backend, string.Empty, Hoy);

        acto.Should().Throw<ViolacionReglaException>()
            .Which.Violaciones.Select(v => v.Mensaje)
            .Should().Contain(TareaReglas.FechaVencimientoPasada);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("esta descripción del título supera los cincuenta caracteres permitidos")]
    public void Crear_rechaza_longitud_de_titulo_invalida(string titulo)
    {
        var acto = () => Tarea.Crear(titulo, "Descripción válida", Hoy.AddDays(2),
            Prioridad.Baja, Categoria.Backend, string.Empty, Hoy);

        acto.Should().Throw<ViolacionReglaException>();
    }

    [Fact]
    public void Crear_rechaza_descripcion_demasiado_larga()
    {
        var descripcion = new string('x', TareaReglas.LongitudDescripcionMaxima + 1);

        var acto = () => Tarea.Crear("Tarea", descripcion, Hoy.AddDays(2),
            Prioridad.Baja, Categoria.Backend, string.Empty, Hoy);

        acto.Should().Throw<ViolacionReglaException>()
            .Which.Violaciones.Select(v => v.Mensaje)
            .Should().Contain(TareaReglas.DescripcionDemasiadoLarga);
    }

    [Fact]
    public void Actualizar_valida_y_cambia_los_campos()
    {
        var tarea = Tarea.Crear("Original", "Descripción válida", Hoy.AddDays(2),
            Prioridad.Baja, Categoria.Backend, string.Empty, Hoy);

        tarea.Actualizar("Nueva", "Otra descripción", Hoy.AddDays(4),
            Prioridad.Media, Categoria.Frontend, "/uploads/x.png", true, Hoy);

        tarea.Titulo.Should().Be("Nueva");
        tarea.Completada.Should().BeTrue();
        tarea.Categoria.Should().Be(Categoria.Frontend);
        tarea.Imagen.Should().Be("/uploads/x.png");
    }

    [Fact]
    public void CambiarCompletada_alterna_el_estado()
    {
        var tarea = Tarea.Crear("Original", "Descripción válida", Hoy.AddDays(2),
            Prioridad.Baja, Categoria.Backend, string.Empty, Hoy);

        tarea.CambiarCompletada(true);

        tarea.Completada.Should().BeTrue();
    }
}
