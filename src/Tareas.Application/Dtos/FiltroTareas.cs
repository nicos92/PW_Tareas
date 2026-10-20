using Tareas.Application.Filtros;
using Tareas.Domain.ValueObjects;

namespace Tareas.Application.Dtos;

/// <summary>
/// Filtro tipado que reemplaza a los strings mágicos
/// ("Completadas", "Vencidas", "VencenHoy", "ATiempo") que viajaban por la interfaz.
/// </summary>
public sealed record FiltroTareas(
    Prioridad? Prioridad = null,
    EstadoFiltro Estado = EstadoFiltro.Todas,
    VencimientoFiltro Vencimiento = VencimientoFiltro.Todos);
