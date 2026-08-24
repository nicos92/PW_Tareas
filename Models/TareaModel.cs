using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TareasBlazor.Validation;

namespace TareasBlazor.Models
{
    public class TareaModel : IValidatableObject
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string IdPublic { get; set; } = string.Empty;

        [Required(ErrorMessage = "El titulo es requerido.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El titulo debe tener entre 3 y 50 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es requerida.")]
        [MaxLength(200, ErrorMessage = "La descripción no puede tener más de 200 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        public bool Completada { get; set; } = false;

        [Required(ErrorMessage = "La fecha de vencimiento es requerida.")]
        [DataType(DataType.Date)]
        [FutureDate(ErrorMessage = "La fecha de vencimiento debe ser futura.")]
        public DateOnly? FechaVencimiento { get; set; }

        [Required(ErrorMessage = "La prioridad es requerida.")]
        [EnumDataType(typeof(Prioridad))]
        public Prioridad Prioridad { get; set; } = Prioridad.Baja;

        [ArchivoImagen(ErrorMessage = "El archivo debe ser una imagen válida (JPG, PNG, GIF).")]
        public string Imagen { get; set; } = string.Empty;

        [EnumDataType(typeof(Categoria))]
        public Categoria Categoria { get; set; } = Categoria.Ninguna;
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Prioridad == Prioridad.Alta && FechaVencimiento > DateOnly.FromDateTime(DateTime.Now.AddDays(7)))
            {
                yield return new ValidationResult("Las tareas de alta prioridad deben tener una fecha de vencimiento menor a 7 días", new[] { nameof(FechaVencimiento) });
            }
        }
    }

    public enum Prioridad
    {
        Baja,
        Media,
        Alta
    }

    public enum Categoria
    {
        Ninguna,
        Database,
        Backend,
        Frontend,
        DevOps,
        Testing
    }
}