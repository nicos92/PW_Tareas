using Microsoft.EntityFrameworkCore;

using Tareas.Application.Abstractions;
using Tareas.Application.Dtos;
using Tareas.Application.Filtros;
using Tareas.Domain.Entidades;
using Tareas.Domain.ValueObjects;
using Tareas.Infrastructure.Persistence;

namespace Tareas.Infrastructure.Repositories;

/// <summary>
/// Única implementación de <see cref="ITareaRepository"/>. Los filtros viven en una sola
/// copia (<see cref="ExtensionesQueryableTarea"/>) y las estadísticas se calculan con
/// agregación en SQL, sin cargar la tabla completa en memoria.
/// </summary>
public sealed class TareaRepository : ITareaRepository
{
    private readonly TareaDbContext _contexto;
    private readonly ISystemClock _reloj;

    public TareaRepository(TareaDbContext contexto, ISystemClock reloj)
    {
        _contexto = contexto;
        _reloj = reloj;
    }

    public async Task<PaginadoResult<Tarea>> ObtenerPaginadoAsync(
        ParametrosPaginacion paginacion,
        FiltroTareas filtro,
        CancellationToken ct = default)
    {
        var query = ExtensionesQueryableTarea.AplicarFiltros(
            _contexto.Tareas.AsNoTracking(), filtro, _reloj.Hoy);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(t => t.Id)
            .Skip((paginacion.Page - 1) * paginacion.PageSize)
            .Take(paginacion.PageSize)
            .ToListAsync(ct);

        return new PaginadoResult<Tarea>(items.AsReadOnly(), totalCount, paginacion.Page, paginacion.PageSize);
    }

    public Task<Tarea?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
    {
        return _contexto.Tareas
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.IdPublic == id, ct);
    }

    public async Task AgregarAsync(Tarea tarea, CancellationToken ct = default)
    {
        await _contexto.Tareas.AddAsync(tarea, ct);
        await _contexto.SaveChangesAsync(ct);
    }

    public async Task ActualizarAsync(Tarea tarea, CancellationToken ct = default)
    {
        _contexto.Tareas.Update(tarea);
        await _contexto.SaveChangesAsync(ct);
    }

    public async Task EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var tarea = await _contexto.Tareas.FirstOrDefaultAsync(t => t.IdPublic == id, ct);
        if (tarea is not null)
        {
            _contexto.Tareas.Remove(tarea);
            await _contexto.SaveChangesAsync(ct);
        }
    }

    public async Task CambiarCompletadaAsync(Guid id, bool completada, CancellationToken ct = default)
    {
        var tarea = await _contexto.Tareas.FirstOrDefaultAsync(t => t.IdPublic == id, ct);
        if (tarea is null)
        {
            return;
        }

        tarea.CambiarCompletada(completada);
        await _contexto.SaveChangesAsync(ct);
    }

    public async Task<EstadisticasTareas> ObtenerEstadisticasAsync(CancellationToken ct = default)
    {
        var hoy = _reloj.Hoy;

        var agregado = await _contexto.Tareas
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Completadas = g.Count(t => t.Completada),
                Baja = g.Count(t => t.Prioridad == Prioridad.Baja),
                Media = g.Count(t => t.Prioridad == Prioridad.Media),
                Alta = g.Count(t => t.Prioridad == Prioridad.Alta),
                Vencidas = g.Count(t => t.FechaVencimiento < hoy),
                VencenHoy = g.Count(t => t.FechaVencimiento == hoy),
                ATiempo = g.Count(t => t.FechaVencimiento > hoy)
            })
            .SingleOrDefaultAsync(ct);

        if (agregado is null)
        {
            return new EstadisticasTareas();
        }

        return new EstadisticasTareas
        {
            Total = agregado.Total,
            Completadas = agregado.Completadas,
            Baja = agregado.Baja,
            Media = agregado.Media,
            Alta = agregado.Alta,
            Vencidas = agregado.Vencidas,
            VencenHoy = agregado.VencenHoy,
            ATiempo = agregado.ATiempo
        };
    }

    public async Task EliminarTodasAsync(CancellationToken ct = default)
    {
        _contexto.Tareas.RemoveRange(_contexto.Tareas);
        await _contexto.SaveChangesAsync(ct);
    }
}

internal static class ExtensionesQueryableTarea
{
    internal static IQueryable<Tarea> AplicarFiltros(
        this IQueryable<Tarea> query,
        FiltroTareas filtro,
        DateOnly hoy)
    {
        if (filtro.Prioridad is { } prioridad)
        {
            query = query.Where(t => t.Prioridad == prioridad);
        }

        query = filtro.Estado switch
        {
            EstadoFiltro.Completadas => query.Where(t => t.Completada),
            EstadoFiltro.Pendientes => query.Where(t => !t.Completada),
            _ => query
        };

        return filtro.Vencimiento switch
        {
            VencimientoFiltro.Vencidas => query.Where(t => t.FechaVencimiento < hoy),
            VencimientoFiltro.VencenHoy => query.Where(t => t.FechaVencimiento == hoy),
            VencimientoFiltro.ATiempo => query.Where(t => t.FechaVencimiento > hoy),
            _ => query
        };
    }
}
