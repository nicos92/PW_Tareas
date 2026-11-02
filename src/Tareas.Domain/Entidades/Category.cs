using Tareas.Domain.Errores;
using Tareas.Domain.Reglas;

namespace Tareas.Domain.Entidades;

/// <summary>
/// Categoría real del dominio. A diferencia del antiguo enum <c>Categoria</c>, ahora es una
/// entidad persistida que puede administrarse en tiempo de ejecución.
/// </summary>
public sealed class Category
{
    private Category()
    {
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public static Category Crear(string nombre, string? descripcion)
    {
        Validar(nombre, descripcion);

        return new Category
        {
            Name = nombre.Trim(),
            Description = descripcion?.Trim() ?? string.Empty
        };
    }

    public void Actualizar(string nombre, string? descripcion)
    {
        Validar(nombre, descripcion);

        Name = nombre.Trim();
        Description = descripcion?.Trim() ?? string.Empty;
    }

    private static void Validar(string nombre, string? descripcion)
    {
        var violaciones = CategoryReglas.Validar(nombre, descripcion);
        if (violaciones.Count > 0)
        {
            throw new ViolacionReglaException(violaciones);
        }
    }
}
