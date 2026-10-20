using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Tareas.Application.Abstractions;
using Tareas.Domain.Entidades;
using Tareas.Domain.ValueObjects;
using Tareas.Infrastructure.Persistence;
using Tareas.Infrastructure.Repositories;

using Xunit;

namespace Tareas.Tests.Integration;

/// <summary>
/// Cubre el conflicto de tracking de EF Core que rompía el guardado: el DbContext es
/// <c>scoped</c> (uno por circuito en Blazor Server) y <c>ActualizarAsync</c> adjuntaba una
/// instancia desconectada de una fila que el contexto ya trackeaba por una operación anterior.
/// </summary>
public class TareaRepositoryActualizarTests : IDisposable
{
    private static readonly DateOnly BaseCreacion = new(2020, 1, 1);
    private static readonly DateOnly Hoy = BaseCreacion.AddDays(30);

    private readonly SqliteConnection _conexion;
    private readonly TareaDbContext _contexto;
    private readonly TareaRepository _repo;

    public TareaRepositoryActualizarTests()
    {
        _conexion = new SqliteConnection("Data Source=:memory:");
        _conexion.Open();

        var opciones = new DbContextOptionsBuilder<TareaDbContext>()
            .UseSqlite(_conexion)
            .Options;

        _contexto = new TareaDbContext(opciones);
        _contexto.Database.EnsureCreated();

        _repo = new TareaRepository(_contexto, new RelojFijo(Hoy));
    }

    private sealed class RelojFijo(DateOnly hoy) : ISystemClock
    {
        public DateTime Ahora { get; } = hoy.ToDateTime(TimeOnly.MinValue);
    }

    /// <summary>
    /// Siembra sin dejar la entidad trackeada, para que el estado inicial sea el de una
    /// sesión recién cargada y cada test controle qué operación previa genera el tracking.
    /// </summary>
    private async Task<Guid> SembrarAsync(string titulo, DateOnly fecha)
    {
        var tarea = Tarea.Crear(titulo, "Descripción válida", fecha, Prioridad.Baja, Categoria.Backend, string.Empty, Hoy);
        _contexto.Tareas.Add(tarea);
        await _contexto.SaveChangesAsync();
        _contexto.Entry(tarea).State = EntityState.Detached;
        return tarea.IdPublic;
    }

    [Fact]
    public async Task Actualizar_dos_veces_en_el_mismo_contexto_no_lanza_y_persiste_el_ultimo_cambio()
    {
        var id = await SembrarAsync("Tarea a editar", Hoy.AddDays(5));

        var primera = await _repo.ObtenerPorIdAsync(id);
        primera!.Actualizar("Tarea a editar", "Descripción válida", Hoy.AddDays(6), Prioridad.Baja, Categoria.Backend, string.Empty, false, Hoy);
        await _repo.ActualizarAsync(primera);

        var segunda = await _repo.ObtenerPorIdAsync(id);
        segunda!.Actualizar("Tarea a editar", "Descripción válida", Hoy.AddDays(7), Prioridad.Baja, Categoria.Backend, string.Empty, false, Hoy);

        Func<Task> accion = () => _repo.ActualizarAsync(segunda);
        await accion.Should().NotThrowAsync("el segundo guardado reutiliza el contexto del circuito");

        var persistida = await _contexto.Tareas.AsNoTracking().SingleAsync(t => t.IdPublic == id);
        persistida.FechaVencimiento.Should().Be(Hoy.AddDays(7));
    }

    [Fact]
    public async Task Actualizar_despues_de_cambiar_completada_no_lanza_conflicto()
    {
        var id = await SembrarAsync("Tarea con toggle", Hoy.AddDays(5));
        await _repo.CambiarCompletadaAsync(id, true);

        var tarea = await _repo.ObtenerPorIdAsync(id);
        tarea!.Actualizar("Tarea con toggle", "Descripción válida", Hoy.AddDays(6), Prioridad.Baja, Categoria.Backend, string.Empty, true, Hoy);

        Func<Task> accion = () => _repo.ActualizarAsync(tarea);
        await accion.Should().NotThrowAsync("el toggle deja la entidad trackeada en el contexto del circuito");

        var persistida = await _contexto.Tareas.AsNoTracking().SingleAsync(t => t.IdPublic == id);
        persistida.Completada.Should().BeTrue();
        persistida.FechaVencimiento.Should().Be(Hoy.AddDays(6));
    }

    [Fact]
    public async Task Actualizar_tras_AgregarAsync_no_lanza_conflicto()
    {
        var creada = Tarea.Crear("Tarea creada", "Descripción válida", Hoy.AddDays(5), Prioridad.Baja, Categoria.Backend, string.Empty, Hoy);
        await _repo.AgregarAsync(creada);

        var cargada = await _repo.ObtenerPorIdAsync(creada.IdPublic);
        cargada!.Actualizar("Tarea creada", "Descripción válida", Hoy.AddDays(6), Prioridad.Baja, Categoria.Backend, string.Empty, false, Hoy);

        Func<Task> accion = () => _repo.ActualizarAsync(cargada);
        await accion.Should().NotThrowAsync("crear deja la entidad trackeada y editarla adjunta otra instancia");

        var persistida = await _contexto.Tareas.AsNoTracking().SingleAsync(t => t.IdPublic == creada.IdPublic);
        persistida.FechaVencimiento.Should().Be(Hoy.AddDays(6));
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }
}
