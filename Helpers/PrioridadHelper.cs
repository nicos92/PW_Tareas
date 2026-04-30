using TareasBlazor.Models;

namespace TareasBlazor.Helpers;

public static class PrioridadHelper
{
    public static string GetClase(Prioridad prioridad) => prioridad switch
    {
        Prioridad.Alta => "bg-danger",
        Prioridad.Media => "bg-warning text-dark",
        Prioridad.Baja => "bg-info text-dark",
        _ => "bg-secondary"
    };
}
