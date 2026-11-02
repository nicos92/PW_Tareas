using Tareas.Application.Abstractions;
using Tareas.Application.Dtos;
using Tareas.Application.Filtros;
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
        IEnumerable<Tarea> query = _tareas.Values;

        query = filtro.EstadoBaja switch
        {
            EstadoBajaFiltro.Activas => query.Where(t => !t.IsDeleted),
            EstadoBajaFiltro.DadasDeBaja => query.Where(t => t.IsDeleted),
            _ => query
        };

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            var termino = filtro.Texto.Trim();
            query = query.Where(t =>
                t.Titulo.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                t.Descripcion.Contains(termino, StringComparison.OrdinalIgnoreCase));
        }

        var items = query.ToList().AsReadOnly();
        return Task.FromResult(new PaginadoResult<Tarea>(
            items, items.Count, paginacion.Page, paginacion.PageSize));
    }

    public Task<Tarea?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_tareas.TryGetValue(id, out var t) && !t.IsDeleted ? t : null);

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

    public Task DarDeBajaAsync(Guid id, CancellationToken ct = default)
    {
        if (_tareas.TryGetValue(id, out var tarea))
        {
            tarea.DarDeBaja();
        }

        return Task.CompletedTask;
    }

    public Task RestaurarAsync(Guid id, CancellationToken ct = default)
    {
        if (_tareas.TryGetValue(id, out var tarea))
        {
            tarea.Restaurar();
        }

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
        => Task.FromResult(new EstadisticasTareas { Total = _tareas.Values.Count(t => !t.IsDeleted) });
}

internal sealed class FakeCategoryRepository : ICategoryRepository
{
    private readonly Dictionary<int, Category> _categorias = new();
    private readonly HashSet<int> _categoriasConTareas = new();
    private int _proximoId = 1;

    public IReadOnlyDictionary<int, Category> Categorias => _categorias;

    public void Sembrar(Category category)
    {
        typeof(Category).GetProperty(nameof(Category.Id))!.SetValue(category, _proximoId++);
        _categorias[category.Id] = category;
    }

    public Task<IReadOnlyList<Category>> ObtenerTodasAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Category>>(
            _categorias.Values.OrderBy(c => c.Name).ToList().AsReadOnly());

    public Task<Category?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        => Task.FromResult(_categorias.TryGetValue(id, out var c) ? c : null);

    public Task<bool> ExisteAsync(int id, CancellationToken ct = default)
        => Task.FromResult(_categorias.ContainsKey(id));

    public void MarcarConTareas(int id) => _categoriasConTareas.Add(id);

    public Task<bool> TieneTareasAsync(int id, CancellationToken ct = default)
        => Task.FromResult(_categoriasConTareas.Contains(id));

    public Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null, CancellationToken ct = default)
    {
        var normalizado = nombre.Trim().ToUpperInvariant();
        return Task.FromResult(_categorias.Values.Any(c =>
            c.Name.ToUpperInvariant() == normalizado && (excluirId is null || c.Id != excluirId)));
    }

    public Task AgregarAsync(Category category, CancellationToken ct = default)
    {
        Sembrar(category);
        return Task.CompletedTask;
    }

    public Task ActualizarAsync(Category category, CancellationToken ct = default)
    {
        _categorias[category.Id] = category;
        return Task.CompletedTask;
    }

    public Task EliminarAsync(int id, CancellationToken ct = default)
    {
        _categorias.Remove(id);
        return Task.CompletedTask;
    }
}
