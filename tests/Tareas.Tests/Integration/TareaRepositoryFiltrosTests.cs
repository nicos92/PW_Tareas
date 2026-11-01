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

public class TareaRepositoryFiltrosTests : IDisposable
{
    // Fecha usada sólo para CREAR tareas (siempre futura respecto de su propio "hoy"),
    // y fecha de referencia del repositorio 30 días después (el "hoy" de los filtros).
    private static readonly DateOnly BaseCreacion = new(2020, 1, 1);
    private static readonly DateOnly Hoy = BaseCreacion.AddDays(30);

    private readonly SqliteConnection _conexion;
    private readonly TareaDbContext _contexto;
    private readonly TareaRepository _repo;

    public TareaRepositoryFiltrosTests()
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

    private void Sembrar(string titulo, Prioridad prioridad, bool completada, DateOnly fecha)
    {
        var tarea = Tarea.Crear(titulo, "Descripción válida", fecha, prioridad, null, string.Empty, BaseCreacion);
        if (completada)
        {
            tarea.CambiarCompletada(true);
        }

        _contexto.Tareas.Add(tarea);
    }

    private void Sembrar(string titulo, string descripcion, Prioridad prioridad = Prioridad.Baja)
    {
        var tarea = Tarea.Crear(titulo, descripcion, BaseCreacion.AddDays(1), prioridad, null, string.Empty, BaseCreacion);
        _contexto.Tareas.Add(tarea);
    }

    [Fact]
    public async Task Filtro_por_prioridad_se_traduce_a_sql()
    {
        _contexto.Tareas.RemoveRange(_contexto.Tareas);
        await _contexto.SaveChangesAsync();
        Sembrar("Baja", Prioridad.Baja, false, BaseCreacion.AddDays(1));
        Sembrar("Alta", Prioridad.Alta, false, BaseCreacion.AddDays(1));
        await _contexto.SaveChangesAsync();

        var resultado = await _repo.ObtenerPaginadoAsync(
            new ParametrosPaginacion(),
            new FiltroTareas(Prioridad: Prioridad.Alta));

        resultado.Items.Should().ContainSingle().Which.Titulo.Should().Be("Alta");
    }

    [Fact]
    public async Task Filtro_por_estado_completadas_se_traduce_a_sql()
    {
        Sembrar("Pendiente", Prioridad.Baja, false, Hoy.AddDays(1));
        Sembrar("Lista", Prioridad.Baja, true, Hoy.AddDays(1));
        await _contexto.SaveChangesAsync();

        var resultado = await _repo.ObtenerPaginadoAsync(
            new ParametrosPaginacion(),
            new FiltroTareas(Estado: EstadoFiltro.Completadas));

        resultado.Items.Should().ContainSingle().Which.Titulo.Should().Be("Lista");
    }

    [Fact]
    public async Task Filtro_vencidas_se_traduce_a_sql()
    {
        Sembrar("Vencida", Prioridad.Baja, false, Hoy.AddDays(-1));
        Sembrar("Futura", Prioridad.Baja, false, Hoy.AddDays(5));
        await _contexto.SaveChangesAsync();

        var resultado = await _repo.ObtenerPaginadoAsync(
            new ParametrosPaginacion(),
            new FiltroTareas(Vencimiento: VencimientoFiltro.Vencidas));

        resultado.Items.Should().ContainSingle().Which.Titulo.Should().Be("Vencida");
    }

    [Fact]
    public async Task Estadisticas_se_calculan_en_sql()
    {
        _contexto.Tareas.RemoveRange(_contexto.Tareas);
        await _contexto.SaveChangesAsync();
        // Alta sólo admite fechas <=7 días desde la creación (BaseCreacion), por eso su
        // fecha es cercana a la base y cuenta como "vencida" frente al reloj del repositorio.
        Sembrar("Alta vencida", Prioridad.Alta, false, BaseCreacion.AddDays(3));
        Sembrar("Media vencida", Prioridad.Media, false, Hoy.AddDays(-1));
        Sembrar("Baja hoy", Prioridad.Baja, false, Hoy);
        Sembrar("Baja futura", Prioridad.Baja, true, Hoy.AddDays(5));
        await _contexto.SaveChangesAsync();

        var stats = await _repo.ObtenerEstadisticasAsync();

        stats.Total.Should().Be(4);
        stats.Completadas.Should().Be(1);
        stats.Alta.Should().Be(1);
        stats.Media.Should().Be(1);
        stats.Baja.Should().Be(2);
        stats.Vencidas.Should().Be(2);
        stats.VencenHoy.Should().Be(1);
        stats.ATiempo.Should().Be(1);
    }

    [Fact]
    public async Task Busqueda_por_titulo_ignora_mayusculas()
    {
        Sembrar("Revisar informe", "Descripción válida");
        Sembrar("Comprar pan", "Descripción válida");
        await _contexto.SaveChangesAsync();

        var resultado = await _repo.ObtenerPaginadoAsync(
            new ParametrosPaginacion(),
            new FiltroTareas(Texto: "INFORME"));

        resultado.Items.Should().ContainSingle().Which.Titulo.Should().Be("Revisar informe");
    }

    [Fact]
    public async Task Busqueda_por_descripcion_filtra_el_listado()
    {
        Sembrar("Tarea A", "Actualizar documentación del API");
        Sembrar("Tarea B", "Preparar presentación");
        await _contexto.SaveChangesAsync();

        var resultado = await _repo.ObtenerPaginadoAsync(
            new ParametrosPaginacion(),
            new FiltroTareas(Texto: "documentación"));

        resultado.Items.Should().ContainSingle().Which.Titulo.Should().Be("Tarea A");
    }

    [Fact]
    public async Task Busqueda_sin_coincidencias_devuelve_listado_vacio()
    {
        Sembrar("Tarea A", "Descripción válida");
        await _contexto.SaveChangesAsync();

        var resultado = await _repo.ObtenerPaginadoAsync(
            new ParametrosPaginacion(),
            new FiltroTareas(Texto: "inexistente"));

        resultado.Items.Should().BeEmpty();
        resultado.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Busqueda_se_combina_con_el_filtro_de_prioridad()
    {
        Sembrar("Informe urgente", "Descripción válida", Prioridad.Alta);
        Sembrar("Informe común", "Descripción válida", Prioridad.Baja);
        await _contexto.SaveChangesAsync();

        var resultado = await _repo.ObtenerPaginadoAsync(
            new ParametrosPaginacion(),
            new FiltroTareas(Prioridad: Prioridad.Alta, Texto: "informe"));

        resultado.Items.Should().ContainSingle().Which.Titulo.Should().Be("Informe urgente");
    }

    [Fact]
    public async Task Tarea_sin_categoria_persiste_la_fk_como_null()
    {
        Sembrar("Sin categoría", Prioridad.Baja, false, Hoy.AddDays(1));
        await _contexto.SaveChangesAsync();

        var fila = await _contexto.Tareas.AsNoTracking().SingleAsync();

        fila.CategoryId.Should().BeNull();
        fila.Category.Should().BeNull();
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }
}
