using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Tareas.Domain.Entidades;
using Tareas.Domain.ValueObjects;
using Tareas.Infrastructure.Persistence;
using Tareas.Infrastructure.Repositories;

using Xunit;

namespace Tareas.Tests.Integration;

public class CategoryRepositoryTests : IDisposable
{
    private static readonly DateOnly Hoy = new(2026, 1, 1);

    private readonly SqliteConnection _conexion;
    private readonly TareaDbContext _contexto;
    private readonly CategoryRepository _categorias;

    public CategoryRepositoryTests()
    {
        _conexion = new SqliteConnection("Data Source=:memory:");
        _conexion.Open();

        var opciones = new DbContextOptionsBuilder<TareaDbContext>()
            .UseSqlite(_conexion)
            .Options;

        _contexto = new TareaDbContext(opciones);
        _contexto.Database.EnsureCreated();
        _categorias = new CategoryRepository(new TestDbContextFactory(opciones));
    }

    [Fact]
    public async Task Eliminar_categoria_con_tareas_falla_por_restriccion_de_fk()
    {
        var categoria = Category.Crear("Backend", "API");
        await _categorias.AgregarAsync(categoria);

        var tarea = Tarea.Crear(
            "Tarea", "Descripción válida", Hoy.AddDays(2),
            Prioridad.Baja, categoria.Id, string.Empty, Hoy);
        _contexto.Tareas.Add(tarea);
        await _contexto.SaveChangesAsync();
        _contexto.ChangeTracker.Clear();

        Func<Task> acto = () => _categorias.EliminarAsync(categoria.Id);

        await acto.Should().ThrowAsync<DbUpdateException>();

        _contexto.ChangeTracker.Clear();
        var persistida = await _contexto.Tareas.AsNoTracking().SingleAsync();
        persistida.CategoryId.Should().Be(categoria.Id);
        (await _categorias.ObtenerTodasAsync()).Should().ContainSingle();
    }

    [Fact]
    public async Task Eliminar_categoria_sin_tareas_la_borra()
    {
        var categoria = Category.Crear("Temporal", string.Empty);
        await _categorias.AgregarAsync(categoria);

        await _categorias.EliminarAsync(categoria.Id);

        (await _categorias.ObtenerTodasAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task ExisteNombre_ignora_mayusculas_y_excluye_la_propia_categoria()
    {
        var categoria = Category.Crear("Backend", string.Empty);
        await _categorias.AgregarAsync(categoria);

        (await _categorias.ExisteNombreAsync("backend")).Should().BeTrue();
        (await _categorias.ExisteNombreAsync("backend", categoria.Id)).Should().BeFalse();
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }
}
