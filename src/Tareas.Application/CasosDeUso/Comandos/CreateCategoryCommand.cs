using Tareas.Application.Abstractions;
using Tareas.Application.Dtos;
using Tareas.Domain.Entidades;

namespace Tareas.Application.CasosDeUso.Comandos;

/// <summary>
/// Crea una categoría validando sus invariantes en el dominio y la unicidad del nombre
/// en la capa de aplicación.
/// </summary>
public sealed class CreateCategoryCommand(ICategoryRepository repositorio)
{
    public async Task<int> EjecutarAsync(CategoryDto dto, CancellationToken ct = default)
    {
        var nombre = dto.Name.Trim();
        if (await repositorio.ExisteNombreAsync(nombre, ct: ct))
        {
            throw new InvalidOperationException($"Ya existe una categoría llamada '{nombre}'.");
        }

        var category = Category.Crear(nombre, dto.Description);
        await repositorio.AgregarAsync(category, ct);
        return category.Id;
    }
}
