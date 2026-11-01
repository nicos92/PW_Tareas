using System.ComponentModel.DataAnnotations;

using Tareas.Domain.Entidades;
using Tareas.Domain.Reglas;

namespace Tareas.Application.Dtos;

/// <summary>
/// Contrato de alta, edición y listado de categorías. Las DataAnnotations quedan en la
/// capa de aplicación para que la entidad de dominio siga siendo pura.
/// </summary>
public class CategoryDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = CategoryReglas.NombreRequerido)]
    [StringLength(CategoryReglas.LongitudNombreMaxima, MinimumLength = CategoryReglas.LongitudNombreMinima,
        ErrorMessage = CategoryReglas.NombreLongitudInvalida)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(CategoryReglas.LongitudDescripcionMaxima, ErrorMessage = CategoryReglas.DescripcionDemasiadoLarga)]
    public string Description { get; set; } = string.Empty;

    public static CategoryDto DesdeDominio(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description
    };
}
