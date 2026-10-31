using FluentAssertions;

using Tareas.Domain.Entidades;
using Tareas.Domain.ValueObjects;

using Xunit;

namespace Tareas.Tests.Domain;

public class TareaBajaTests
{
    private static readonly DateOnly Hoy = new(2026, 1, 1);

    private static Tarea CrearTarea() => Tarea.Crear(
        "Tarea", "Descripción válida", Hoy.AddDays(2),
        Prioridad.Baja, Categoria.Backend, string.Empty, Hoy);

    [Fact]
    public void DarDeBaja_marca_la_tarea_como_eliminada()
    {
        var tarea = CrearTarea();

        tarea.DarDeBaja();

        tarea.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void DarDeBaja_es_idempotente()
    {
        var tarea = CrearTarea();

        tarea.DarDeBaja();
        tarea.DarDeBaja();

        tarea.IsDeleted.Should().BeTrue();
    }
}
