using System.ComponentModel.DataAnnotations;

namespace Tareas.Application.Validation;

public class ArchivoImagenAttribute : ValidationAttribute
{
    public static readonly IReadOnlyList<string> ExtensionesPermitidas =
        Array.AsReadOnly(new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" });

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string fileName && !string.IsNullOrEmpty(fileName))
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension))
            {
                return new ValidationResult(ErrorMessage ?? "El archivo debe ser una imagen válida (JPG, PNG, GIF).");
            }
        }
        return ValidationResult.Success;
    }
}
