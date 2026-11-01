using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Tareas.Application.Abstractions;
using Tareas.Application.Dtos;
using Tareas.Application.Filtros;
using Tareas.Domain.Entidades;
using Tareas.Domain.ValueObjects;
using Tareas.Infrastructure.Persistence;
using Tareas.Infrastructure.Repositories;

using Xunit;

namespace Tareas.Tests.Integration;

public class TareaRepositoryBajaTests : IDisposable
{
    private static readonly DateOnly BaseCreacion = new(2020, 1, 1);
    private static readonly DateOnly Hoy = BaseCreacion.AddDays(30);

    private readonly SqliteConnection _conexion;
    private readonly TareaDbContext _contexto;
    private readonly TareaRepository _repo;

    public TareaRepositoryBajaTests()
    {
        _conexion = new SqliteConnection("Data Source=:memory:");
        _conexion.Open();

        var opciones = new DbContextOptionsBuilder<TareaDbContext>()
            .UseSqlite(_conexion)
            .Options;

        _contexto = new TareaDbContext(opciones);
        _contexto.Database.EnsureCreated();

        _repo = new TareaRepository(new TestDbContextFactory(opciones), new RelojFijo(Hoy));
    }

    private sealed class RelojFijo(DateOnly hoy) : ISystemClock
    {
        public DateTime Ahora { get; } = hoy.ToDateTime(TimeOnly.MinValue);
    }

    private async Task<Guid> SembrarAsync(string titulo)
    {
        var tarea = Tarea.Crear(titulo, "Descripción válida", Hoy.AddDays(5),
            Prioridad.Baja, null, string.Empty, BaseCreacion);
        _contexto.Tareas.Add(tarea);
        await _contexto.SaveChangesAsync();
        return tarea.IdPublic;
    }

    [Fact]
    public async Task DarDeBaja_excluye_la_tarea_de_listado_lookup_y_estadisticas()
    {
        var id = await SembrarAsync("A dar de baja");
        await SembrarAsync("Activa");

        await _repo.DarDeBajaAsync(id);

        (await _repo.ObtenerPorIdAsync(id)).Should().BeNull();

        var listado = await _repo.ObtenerPaginadoAsync(
            new ParametrosPaginacion(), new FiltroTareas());
        listado.Items.Should().ContainSingle().Which.Titulo.Should().Be("Activa");

        var estadisticas = await _repo.ObtenerEstadisticasAsync();
        estadisticas.Total.Should().Be(1);

        var filas = await _contexto.Tareas.IgnoreQueryFilters().AsNoTracking().ToListAsync();
        filas.Should().HaveCount(2);
        filas.Single(t => t.IdPublic == id).IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DarDeBaja_es_idempotente()
    {
        var id = await SembrarAsync("A dar de baja dos veces");

        await _repo.DarDeBajaAsync(id);
        var acto = () => _repo.DarDeBajaAsync(id);

        await acto.Should().NotThrowAsync();

        var fila = await _contexto.Tareas.IgnoreQueryFilters().AsNoTracking().SingleAsync(t => t.IdPublic == id);
        fila.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Listado_puede_filtrar_solo_bajas_o_incluir_todas()
    {
        var idBaja = await SembrarAsync("Baja");
        var idActiva = await SembrarAsync("Activa");
        await _repo.DarDeBajaAsync(idBaja);

        var soloBajas = await _repo.ObtenerPaginadoAsync(
            new ParametrosPaginacion(),
            new FiltroTareas(EstadoBaja: EstadoBajaFiltro.DadasDeBaja));

        soloBajas.Items.Should().ContainSingle()
            .Which.IdPublic.Should().Be(idBaja);

        var todas = await _repo.ObtenerPaginadoAsync(
            new ParametrosPaginacion(),
            new FiltroTareas(EstadoBaja: EstadoBajaFiltro.Todas));

        todas.Items.Should().HaveCount(2);
        todas.Items.Select(t => t.IdPublic).Should().BeEquivalentTo(new[] { idBaja, idActiva });
    }

    [Fact]
    public async Task Restaurar_reactiva_la_tarea_y_la_devuelve_al_listado_activo()
    {
        var id = await SembrarAsync("A restaurar");
        await _repo.DarDeBajaAsync(id);

        await _repo.RestaurarAsync(id);

        (await _repo.ObtenerPorIdAsync(id)).Should().NotBeNull();

        var listado = await _repo.ObtenerPaginadoAsync(
            new ParametrosPaginacion(),
            new FiltroTareas());

        listado.Items.Should().ContainSingle()
            .Which.IdPublic.Should().Be(id);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }
}
