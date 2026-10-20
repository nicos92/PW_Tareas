using Tareas.Application.Dtos;

namespace TareasBlazor.Helpers;

public enum EstadoVisual
{
    Pendiente,
    Completada,
    Vencida,
    PorVencer
}

/// <summary>
/// Calcula el estado visual de una tarea (completada, vencida, por vencer o pendiente)
/// y sus clases de estilo asociadas, para que los componentes no repitan la lógica.
/// </summary>
public static class EstadoTareaHelper
{
    private const int DiasUmbralPorVencer = 2;

    public static EstadoVisual GetEstado(TareaDto tarea, DateOnly? hoy = null)
    {
        var fechaActual = hoy ?? DateOnly.FromDateTime(DateTime.Now);

        if (tarea.Completada)
        {
            return EstadoVisual.Completada;
        }

        if (tarea.FechaVencimiento is not { } fecha)
        {
            return EstadoVisual.Pendiente;
        }

        if (fecha < fechaActual)
        {
            return EstadoVisual.Vencida;
        }

        return fecha.DayNumber - fechaActual.DayNumber <= DiasUmbralPorVencer
            ? EstadoVisual.PorVencer
            : EstadoVisual.Pendiente;
    }

    public static string GetClaseBadge(EstadoVisual estado) => estado switch
    {
        EstadoVisual.Completada => "badge-completada",
        EstadoVisual.Vencida => "badge-alta",
        EstadoVisual.PorVencer => "badge-media",
        _ => "badge-pendiente"
    };

    public static string GetEtiqueta(EstadoVisual estado) => estado switch
    {
        EstadoVisual.Completada => "Completada",
        EstadoVisual.Vencida => "Vencida",
        EstadoVisual.PorVencer => "Por vencer",
        _ => "Pendiente"
    };

    public static string GetClaseSuperficie(EstadoVisual estado) => estado switch
    {
        EstadoVisual.Completada => "surface-tint-tertiary",
        EstadoVisual.Vencida => "surface-tint-error",
        EstadoVisual.PorVencer => "surface-tint-warning",
        _ => "surface-tint-secondary"
    };
}
