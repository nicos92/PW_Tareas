using Tareas.Application.Abstractions;

namespace Tareas.Infrastructure.Time;

/// <summary>
/// Reloj real de la aplicación. La capa de negocio nunca llama a <c>DateTime.Now</c>
/// directamente, lo que hace testeables la regla de 7 días y los filtros de vencimiento.
/// </summary>
public sealed class SystemClock : ISystemClock
{
    public DateTime Ahora => DateTime.Now;
}
