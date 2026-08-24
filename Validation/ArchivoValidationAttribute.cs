using System.ComponentModel.DataAnnotations;

namespace TareasBlazor.Validation
{
    public class ArchivoImagenAttribute : ValidationAttribute
    {
        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

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

    public class PesoArchivoAttribute : ValidationAttribute
    {
        public long MaxBytes { get; set; } = 1024 * 1024;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            return ValidationResult.Success;
        }
    }
}