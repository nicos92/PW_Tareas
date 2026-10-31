using System.ComponentModel.DataAnnotations;

using FluentAssertions;

using Tareas.Application.Validation;

using Xunit;

namespace Tareas.Tests.Application;

public class ArchivoImagenAttributeTests
{
    [Theory]
    [InlineData("foto.jpg", true)]
    [InlineData("foto.jpeg", true)]
    [InlineData("foto.png", true)]
    [InlineData("foto.gif", true)]
    [InlineData("foto.webp", true)]
    [InlineData("foto.svg", false)]
    [InlineData("foto.exe", false)]
    [InlineData("", false)]
    public void EsExtensionPermitida_solo_acepta_formatos_de_imagen_soportados(
        string nombreArchivo, bool esperado)
    {
        ArchivoImagenAttribute.EsExtensionPermitida(nombreArchivo).Should().Be(esperado);
    }

    [Fact]
    public void IsValid_rechaza_svg_con_el_mensaje_de_formatos_permitidos()
    {
        var attribute = new ArchivoImagenAttribute();

        var resultado = attribute.GetValidationResult("foto.svg", new ValidationContext(new object()));

        resultado.Should().NotBeNull();
        resultado!.ErrorMessage.Should().Contain(ArchivoImagenAttribute.FormatosPermitidosTexto);
    }

    [Fact]
    public void IsValid_acepta_png()
    {
        var attribute = new ArchivoImagenAttribute();

        var resultado = attribute.GetValidationResult("foto.png", new ValidationContext(new object()));

        resultado.Should().Be(ValidationResult.Success);
    }
}
