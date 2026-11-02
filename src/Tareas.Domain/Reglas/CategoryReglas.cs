using Tareas.Domain.Errores;

namespace Tareas.Domain.Reglas;

/// <summary>
/// Invariantes de la entidad <see cref="Entidades.Category"/>. Los textos viven acá como
/// fuente única para que la UI y el dominio muestren exactamente el mismo mensaje.
/// </summary>
public static class CategoryReglas
{
    public const int LongitudNombreMinima = 2;
    public const int LongitudNombreMaxima = 100;
    public const int LongitudDescripcionMaxima = 250;

    public const string NombreRequerido = "El nombre de la categoría es requerido.";
    public const string NombreLongitudInvalida =
        "El nombre de la categoría debe tener entre 2 y 100 caracteres.";
    public const string DescripcionDemasiadoLarga =
        "La descripción de la categoría no puede tener más de 250 caracteres.";

    public static IReadOnlyList<ViolacionRegla> Validar(string nombre, string? descripcion)
    {
        var violaciones = new List<ViolacionRegla>();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            violaciones.Add(new ViolacionRegla(nameof(nombre), NombreRequerido));
        }
        else if (nombre.Trim().Length < LongitudNombreMinima
                 || nombre.Trim().Length > LongitudNombreMaxima)
        {
            violaciones.Add(new ViolacionRegla(nameof(nombre), NombreLongitudInvalida));
        }

        if (!string.IsNullOrWhiteSpace(descripcion)
            && descripcion.Trim().Length > LongitudDescripcionMaxima)
        {
            violaciones.Add(new ViolacionRegla(nameof(descripcion), DescripcionDemasiadoLarga));
        }

        return violaciones;
    }
}
