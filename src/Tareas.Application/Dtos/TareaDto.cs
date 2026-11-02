using System.ComponentModel.DataAnnotations;

using Tareas.Application.Validation;
using Tareas.Domain.Entidades;
using Tareas.Domain.Reglas;
using Tareas.Domain.ValueObjects;

namespace Tareas.Application.Dtos;

/// <summary>
/// Contrato de la UI. Las <c>DataAnnotations</c> viven acá —no en la entidad— para que el
/// esquema SQL deje de depender de la validación del formulario. Los textos de error
/// provienen de <see cref="TareaReglas"/>, la fuente única de mensajes.
/// </summary>
public class TareaDto : IValidatableObject
{
    public Guid IdPublic { get; set; }

    [Required(ErrorMessage = TareaReglas.TituloRequerido)]
    [StringLength(TareaReglas.LongitudTituloMaxima, MinimumLength = TareaReglas.LongitudTituloMinima,
        ErrorMessage = TareaReglas.TituloLongitudInvalida)]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = TareaReglas.DescripcionRequerida)]
    [MaxLength(TareaReglas.LongitudDescripcionMaxima, ErrorMessage = TareaReglas.DescripcionDemasiadoLarga)]
    public string Descripcion { get; set; } = string.Empty;

    public bool Completada { get; set; }

    public bool IsDeleted { get; set; }

    [Required(ErrorMessage = TareaReglas.FechaVencimientoRequerida)]
    [DataType(DataType.Date)]
    [FutureDate(ErrorMessage = TareaReglas.FechaVencimientoPasada)]
    public DateOnly? FechaVencimiento { get; set; }

    [Required(ErrorMessage = "La prioridad es requerida.")]
    [EnumDataType(typeof(Prioridad))]
    public Prioridad Prioridad { get; set; } = Prioridad.Baja;

    [ArchivoImagen(ErrorMessage = ArchivoImagenAttribute.MensajeFormatoInvalido)]
    public string Imagen { get; set; } = string.Empty;

    public int? CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Prioridad == Prioridad.Alta
            && FechaVencimiento > DateOnly.FromDateTime(DateTime.Now.AddDays(TareaReglas.DiasLimitePrioridadAlta)))
        {
            yield return new ValidationResult(
                TareaReglas.PrioridadAltaLimiteSieteDias, new[] { nameof(FechaVencimiento) });
        }
    }

    public static TareaDto DesdeDominio(Tarea tarea) => new()
    {
        IdPublic = tarea.IdPublic,
        Titulo = tarea.Titulo,
        Descripcion = tarea.Descripcion,
        Completada = tarea.Completada,
        IsDeleted = tarea.IsDeleted,
        FechaVencimiento = tarea.FechaVencimiento,
        Prioridad = tarea.Prioridad,
        Imagen = tarea.Imagen,
        CategoryId = tarea.CategoryId,
        CategoryName = tarea.Category?.Name
    };
}
