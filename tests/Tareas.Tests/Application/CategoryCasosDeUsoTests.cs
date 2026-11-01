using FluentAssertions;

using Tareas.Application.CasosDeUso.Comandos;
using Tareas.Application.CasosDeUso.Consultas;
using Tareas.Application.Dtos;
using Tareas.Domain.Entidades;
using Tareas.Domain.ValueObjects;

using Xunit;

namespace Tareas.Tests.Application;

public class CategoryCasosDeUsoTests
{
    private static readonly DateOnly Hoy = new(2026, 1, 1);

    [Fact]
    public async Task Crear_categoria_la_persiste_y_la_devuelve_en_el_listado()
    {
        var repo = new FakeCategoryRepository();
        var cmd = new CreateCategoryCommand(repo);

        var id = await cmd.EjecutarAsync(new CategoryDto
        {
            Name = "Backend",
            Description = "Servicios y API"
        });

        id.Should().BePositive();

        var listado = await new GetCategoriesQuery(repo).EjecutarAsync();
        listado.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { Id = id, Name = "Backend", Description = "Servicios y API" });
    }

    [Fact]
    public async Task Actualizar_categoria_cambia_los_datos()
    {
        var repo = new FakeCategoryRepository();
        var categoria = Category.Crear("Backend", "Original");
        repo.Sembrar(categoria);

        var cmd = new UpdateCategoryCommand(repo);
        await cmd.EjecutarAsync(categoria.Id, new CategoryDto
        {
            Name = "API",
            Description = "Actualizada"
        });

        var dto = await new GetCategoryByIdQuery(repo).EjecutarAsync(categoria.Id);
        dto!.Name.Should().Be("API");
        dto.Description.Should().Be("Actualizada");
    }

    [Fact]
    public async Task Eliminar_categoria_la_quita_del_listado()
    {
        var repo = new FakeCategoryRepository();
        var categoria = Category.Crear("Temporal", string.Empty);
        repo.Sembrar(categoria);

        await new DeleteCategoryCommand(repo).EjecutarAsync(categoria.Id);

        (await new GetCategoriesQuery(repo).EjecutarAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task Crear_categoria_rechaza_nombre_duplicado()
    {
        var repo = new FakeCategoryRepository();
        repo.Sembrar(Category.Crear("Backend", string.Empty));

        var acto = () => new CreateCategoryCommand(repo).EjecutarAsync(new CategoryDto { Name = "backend" });

        await acto.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Crear_tarea_rechaza_una_categoria_inexistente()
    {
        var tareas = new FakeTareaRepository();
        var categorias = new FakeCategoryRepository();
        var cmd = new CrearTareaCommand(tareas, categorias, new FakeArchivoStorage(), new FakeClock(Hoy));

        var dto = new TareaDto
        {
            Titulo = "Tarea",
            Descripcion = "Descripción válida",
            FechaVencimiento = Hoy.AddDays(2),
            Prioridad = Prioridad.Baja,
            CategoryId = 99
        };

        var acto = () => cmd.EjecutarAsync(dto, imagen: null);

        await acto.Should().ThrowAsync<InvalidOperationException>();
        tareas.Tareas.Should().BeEmpty();
    }

    [Fact]
    public async Task Crear_tarea_persiste_la_categoria_opcional()
    {
        var tareas = new FakeTareaRepository();
        var categorias = new FakeCategoryRepository();
        var categoria = Category.Crear("Backend", string.Empty);
        categorias.Sembrar(categoria);

        var cmd = new CrearTareaCommand(tareas, categorias, new FakeArchivoStorage(), new FakeClock(Hoy));
        var dto = new TareaDto
        {
            Titulo = "Tarea",
            Descripcion = "Descripción válida",
            FechaVencimiento = Hoy.AddDays(2),
            Prioridad = Prioridad.Baja,
            CategoryId = categoria.Id
        };

        var id = await cmd.EjecutarAsync(dto, imagen: null);

        (await tareas.ObtenerPorIdAsync(id))!.CategoryId.Should().Be(categoria.Id);
    }

    [Fact]
    public async Task Crear_tarea_sin_categoria_es_valido()
    {
        var tareas = new FakeTareaRepository();
        var categorias = new FakeCategoryRepository();
        var cmd = new CrearTareaCommand(tareas, categorias, new FakeArchivoStorage(), new FakeClock(Hoy));

        var dto = new TareaDto
        {
            Titulo = "Tarea",
            Descripcion = "Descripción válida",
            FechaVencimiento = Hoy.AddDays(2),
            Prioridad = Prioridad.Baja,
            CategoryId = null
        };

        var id = await cmd.EjecutarAsync(dto, imagen: null);

        (await tareas.ObtenerPorIdAsync(id))!.CategoryId.Should().BeNull();
    }
}
