using Tareas.Application.CasosDeUso.Comandos;
using Tareas.Application.CasosDeUso.Consultas;
using Tareas.Application.Dtos;
using Tareas.Application.Filtros;
using Tareas.Domain.ValueObjects;

namespace TareasBlazor.Components.Features.Tareas;

/// <summary>
/// Estado de presentación de la lista de tareas. Conserva <b>sólo</b> estado de UI
/// (filtros, página, carga) y delega toda la lógica en los casos de uso de Application.
/// Ya no conoce <c>IWebHostEnvironment</c> ni realiza I/O de archivos.
/// </summary>
public sealed class TareaEstado
{
    private readonly ObtenerTareasQuery _obtenerTareas;
    private readonly ObtenerTareaPorIdQuery _obtenerPorId;
    private readonly ObtenerEstadisticasQuery _obtenerEstadisticas;
    private readonly CrearTareaCommand _crearTarea;
    private readonly ActualizarTareaCommand _actualizarTarea;
    private readonly EliminarTareaCommand _eliminarTarea;
    private readonly CambiarCompletadaTareaCommand _cambiarCompletada;

    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _inicializado;

    public TareaEstado(
        ObtenerTareasQuery obtenerTareas,
        ObtenerTareaPorIdQuery obtenerPorId,
        ObtenerEstadisticasQuery obtenerEstadisticas,
        CrearTareaCommand crearTarea,
        ActualizarTareaCommand actualizarTarea,
        EliminarTareaCommand eliminarTarea,
        CambiarCompletadaTareaCommand cambiarCompletada)
    {
        _obtenerTareas = obtenerTareas;
        _obtenerPorId = obtenerPorId;
        _obtenerEstadisticas = obtenerEstadisticas;
        _crearTarea = crearTarea;
        _actualizarTarea = actualizarTarea;
        _eliminarTarea = eliminarTarea;
        _cambiarCompletada = cambiarCompletada;
    }

    public PaginadoResult<TareaDto>? ResultadoPaginado { get; private set; }
    public EstadisticasTareas Estadisticas { get; private set; } = new();
    public ParametrosPaginacion PaginacionActual { get; } = new();

    public FiltroTareas Filtro { get; private set; } = new();

    public bool IsLoading { get; private set; }
    public event Action? OnChange;

    public string FiltroPrioridadTexto =>
        Filtro.Prioridad?.ToString() ?? string.Empty;
    public string FiltroEstadoTexto => Filtro.Estado.ToString();
    public string FiltroVencimientoTexto => Filtro.Vencimiento.ToString();

    public async Task Inicializar()
    {
        if (_inicializado)
        {
            return;
        }

        await _initLock.WaitAsync();
        try
        {
            if (_inicializado)
            {
                return;
            }

            _inicializado = true;
        }
        finally
        {
            _initLock.Release();
        }

        await CargarPaginaAsync();
    }

    public async Task CargarPaginaAsync()
    {
        IsLoading = true;
        NotificarCambio();

        var pagina = _obtenerTareas.EjecutarAsync(PaginacionActual, Filtro);
        var estadisticas = _obtenerEstadisticas.EjecutarAsync();

        await Task.WhenAll(pagina, estadisticas);

        ResultadoPaginado = await pagina;
        Estadisticas = await estadisticas;

        IsLoading = false;
        NotificarCambio();
    }

    public Task SetFiltroPrioridadAsync(string? prioridad)
    {
        Filtro = Filtro with
        {
            Prioridad = Enum.TryParse<Prioridad>(prioridad, out var p) ? p : null
        };

        return ReiniciarYCargarAsync();
    }

    public Task SetFiltroEstadoAsync(string? estado)
    {
        Filtro = Filtro with
        {
            Estado = Enum.TryParse<EstadoFiltro>(estado, out var e) ? e : EstadoFiltro.Todas
        };

        return ReiniciarYCargarAsync();
    }

    public Task SetFiltroVencimientoAsync(string? vencimiento)
    {
        Filtro = Filtro with
        {
            Vencimiento = Enum.TryParse<VencimientoFiltro>(vencimiento, out var v) ? v : VencimientoFiltro.Todos
        };

        return ReiniciarYCargarAsync();
    }

    private Task ReiniciarYCargarAsync()
    {
        PaginacionActual.Page = 1;
        return CargarPaginaAsync();
    }

    public Task IrAPaginaAsync(int page)
    {
        PaginacionActual.Page = page;
        return CargarPaginaAsync();
    }

    public async Task<Guid> AgregarTareaAsync(TareaDto dto, SubidaArchivo? imagen)
    {
        var id = await _crearTarea.EjecutarAsync(dto, imagen);
        await CargarPaginaAsync();
        return id;
    }

    public async Task ActualizarTareaAsync(Guid id, TareaDto dto, SubidaArchivo? imagen)
    {
        await _actualizarTarea.EjecutarAsync(id, dto, imagen);
        await CargarPaginaAsync();
    }

    public async Task EliminarTareaAsync(Guid id)
    {
        await _eliminarTarea.EjecutarAsync(id);

        if (ResultadoPaginado is not null && ResultadoPaginado.Items.Count == 1 && PaginacionActual.Page > 1)
        {
            PaginacionActual.Page--;
        }

        await CargarPaginaAsync();
    }

    public async Task ToggleAsync(Guid id)
    {
        await _cambiarCompletada.EjecutarAsync(id);
        await CargarPaginaAsync();
    }

    public async Task<TareaDto?> ObtenerPorIdAsync(string id)
    {
        return Guid.TryParse(id, out var guid)
            ? await _obtenerPorId.EjecutarAsync(guid)
            : null;
    }

    private void NotificarCambio() => OnChange?.Invoke();
}
