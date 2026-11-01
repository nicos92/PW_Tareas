using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Tareas.Domain.Entidades;
using Tareas.Infrastructure.Persistence;

using Xunit;

namespace Tareas.Tests.Integration;

public class SeedDataTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly TareaDbContext _contexto;

    public SeedDataTests()
    {
        _conexion = new SqliteConnection("Data Source=:memory:");
        _conexion.Open();

        var opciones = new DbContextOptionsBuilder<TareaDbContext>()
            .UseSqlite(_conexion)
            .Options;

        _contexto = new TareaDbContext(opciones);
        _contexto.Database.EnsureCreated();
    }

    [Fact]
    public async Task Inicializar_dos_veces_no_duplica_categorias()
    {
        await SeedData.InicializarAsync(_contexto);
        var cantidadInicial = await _contexto.Categorias.CountAsync();

        await SeedData.InicializarAsync(_contexto);

        (await _contexto.Categorias.CountAsync()).Should().Be(cantidadInicial);
        cantidadInicial.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Inicializar_respeta_categorias_existentes_ignorando_mayusculas()
    {
        _contexto.Categorias.Add(Category.Crear("trabajo", "Descripción existente"));
        await _contexto.SaveChangesAsync();

        await SeedData.InicializarAsync(_contexto);

        var trabajos = await _contexto.Categorias
            .Where(c => c.Name.ToUpper() == "TRABAJO")
            .ToListAsync();

        trabajos.Should().ContainSingle();
        trabajos[0].Description.Should().Be("Descripción existente");
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }
}
