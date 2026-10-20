using Tareas.Application.Abstractions;
using Tareas.Application.Dtos;
using Tareas.Domain.Entidades;

namespace Tareas.Tests.Application;

internal sealed class FakeClock : ISystemClock
{
    public FakeClock(DateOnly hoy)
    {
        Ahora = hoy.ToDateTime(TimeOnly.MinValue);
    }

    public DateTime Ahora { get; set; }
}

internal sealed class FakeArchivoStorage : IArchivoStorage
{
    public List<string> Guardados { get; } = new();
    public List<string> Eliminados { get; } = new();

    public Task<string> GuardarAsync(Stream contenido, string extension, CancellationToken ct = default)
    {
        var ruta = $"/uploads/fake{extension}";
        Guardados.Add(ruta);
        return Task.FromResult(ruta);
    }

    public Task EliminarAsync(string rutaRelativa, CancellationToken ct = default)
    {
        Eliminados.Add(rutaRelativa);
        return Task.CompletedTask;
    }
}

internal sealed class FakeTareaRepository : ITareaRepository
{
    private readonly Dictionary<Guid, Tarea> _tareas = new();
    private int _proximoId = 1;

    public IReadOnlyDictionary<Guid, Tarea> Tareas => _tareas;

    public void Sembrar(Tarea tarea)
    {
        typeof(Tarea).GetProperty(nameof(Tarea.Id))!.SetValue(tarea, _proximoId++);
        _tareas[tarea.IdPublic] = tarea;
    }

    public Task<PaginadoResult<Tarea>> ObtenerPaginadoAsync(
        ParametrosPaginacion paginacion, FiltroTareas filtro, CancellationToken ct = default)
    {
        var items = _tareas.Values.ToList().AsReadOnly();
        return Task.FromResult(new PaginadoResult<Tarea>(
            items, items.Count, paginacion.Page, paginacion.PageSize));
    }

    public Task<Tarea?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_tareas.TryGetValue(id, out var t) ? t : null);

    public Task AgregarAsync(Tarea tarea, CancellationToken ct = default)
    {
        Sembrar(tarea);
        return Task.CompletedTask;
    }

    public Task ActualizarAsync(Tarea tarea, CancellationToken ct = default)
    {
        _tareas[tarea.IdPublic] = tarea;
        return Task.CompletedTask;
    }

    public Task EliminarAsync(Guid id, CancellationToken ct = default)
    {
        _tareas.Remove(id);
        return Task.CompletedTask;
    }

    public Task CambiarCompletadaAsync(Guid id, bool completada, CancellationToken ct = default)
    {
        if (_tareas.TryGetValue(id, out var t))
        {
            t.CambiarCompletada(completada);
        }

        return Task.CompletedTask;
    }

    public Task<EstadisticasTareas> ObtenerEstadisticasAsync(CancellationToken ct = default)
        => Task.FromResult(new EstadisticasTareas { Total = _tareas.Count });

    public Task EliminarTodasAsync(CancellationToken ct = default)
    {
        _tareas.Clear();
        return Task.CompletedTask;
    }
}
