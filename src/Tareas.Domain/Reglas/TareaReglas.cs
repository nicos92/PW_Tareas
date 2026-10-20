using Tareas.Domain.Errores;
using Tareas.Domain.ValueObjects;

namespace Tareas.Domain.Reglas;

/// <summary>
/// Invariantes de la entidad <see cref="Entidades.Tarea"/>. Los textos en español viven acá
/// como fuente única: la UI los muestra directamente.
/// </summary>
public static class TareaReglas
{
    public const int LongitudTituloMinima = 3;
    public const int LongitudTituloMaxima = 50;
    public const int LongitudDescripcionMaxima = 200;
    public const int DiasLimitePrioridadAlta = 7;

    public const string TituloRequerido = "El titulo es requerido.";
    public const string TituloLongitudInvalida = "El titulo debe tener entre 3 y 50 caracteres.";
    public const string DescripcionRequerida = "La descripción es requerida.";
    public const string DescripcionDemasiadoLarga = "La descripción no puede tener más de 200 caracteres.";
    public const string FechaVencimientoRequerida = "La fecha de vencimiento es requerida.";
    public const string FechaVencimientoPasada = "La fecha de vencimiento debe ser futura.";
    public const string PrioridadAltaLimiteSieteDias =
        "Las tareas de alta prioridad deben tener una fecha de vencimiento menor a 7 días";

    public static IReadOnlyList<ViolacionRegla> Validar(
        string titulo,
        string? descripcion,
        DateOnly? fechaVencimiento,
        Prioridad prioridad,
        DateOnly hoy)
    {
        var violaciones = new List<ViolacionRegla>();

        if (string.IsNullOrWhiteSpace(titulo))
        {
            violaciones.Add(new ViolacionRegla(nameof(titulo), TituloRequerido));
        }
        else if (titulo.Length < LongitudTituloMinima || titulo.Length > LongitudTituloMaxima)
        {
            violaciones.Add(new ViolacionRegla(nameof(titulo), TituloLongitudInvalida));
        }

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            violaciones.Add(new ViolacionRegla(nameof(descripcion), DescripcionRequerida));
        }
        else if (descripcion.Length > LongitudDescripcionMaxima)
        {
            violaciones.Add(new ViolacionRegla(nameof(descripcion), DescripcionDemasiadoLarga));
        }

        if (fechaVencimiento is null)
        {
            violaciones.Add(new ViolacionRegla(nameof(fechaVencimiento), FechaVencimientoRequerida));
        }
        else if (fechaVencimiento < hoy)
        {
            violaciones.Add(new ViolacionRegla(nameof(fechaVencimiento), FechaVencimientoPasada));
        }

        if (prioridad == Prioridad.Alta
            && fechaVencimiento is not null
            && fechaVencimiento > hoy.AddDays(DiasLimitePrioridadAlta))
        {
            violaciones.Add(new ViolacionRegla(nameof(fechaVencimiento), PrioridadAltaLimiteSieteDias));
        }

        return violaciones;
    }
}
