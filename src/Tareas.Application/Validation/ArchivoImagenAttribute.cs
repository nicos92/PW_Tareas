using System.ComponentModel.DataAnnotations;

namespace Tareas.Application.Validation;

public class ArchivoImagenAttribute : ValidationAttribute
{
    public const string FormatosPermitidosTexto = "JPG, JPEG, PNG, GIF o WEBP";
    public const string MensajeFormatoInvalido =
        "El archivo debe ser una imagen válida (" + FormatosPermitidosTexto + ").";

    public static readonly IReadOnlyList<string> ExtensionesPermitidas =
        Array.AsReadOnly(new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" });

    public static bool EsExtensionPermitida(string? nombreArchivo)
    {
        if (string.IsNullOrWhiteSpace(nombreArchivo))
        {
            return false;
        }

        var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();
        return ExtensionesPermitidas.Contains(extension);
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string fileName && !string.IsNullOrEmpty(fileName) && !EsExtensionPermitida(fileName))
        {
            return new ValidationResult(ErrorMessage ?? MensajeFormatoInvalido);
        }
        return ValidationResult.Success;
    }
}
