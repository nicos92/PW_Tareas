using TareasBlazor.Models;

namespace TareasBlazor.Helpers;

public static class PrioridadHelper
{
    public static string GetClase(Prioridad prioridad) => prioridad switch
    {
        Prioridad.Alta => "badge-alta",
        Prioridad.Media => "badge-media",
        Prioridad.Baja => "badge-baja",
        _ => "bg-secondary"
    };
}