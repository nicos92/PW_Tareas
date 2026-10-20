namespace Tareas.Application.Abstractions;

/// <summary>
/// Puerto que abstrae el reloj de sistema. Hace testeables la regla de 7 días de
/// prioridad Alta y los filtros "Vencidas" / "Vencen hoy" / "A tiempo".
/// </summary>
public interface ISystemClock
{
    DateTime Ahora { get; }

    DateOnly Hoy => DateOnly.FromDateTime(Ahora);
}
