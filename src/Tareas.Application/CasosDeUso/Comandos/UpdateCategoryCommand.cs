using Tareas.Application.Abstractions;
using Tareas.Application.Dtos;

namespace Tareas.Application.CasosDeUso.Comandos;

/// <summary>
/// Actualiza una categoría existente. Valida la unicidad del nombre excluyendo la propia
/// categoría para permitir guardar sin cambios.
/// </summary>
public sealed class UpdateCategoryCommand(ICategoryRepository repositorio)
{
    public async Task EjecutarAsync(int id, CategoryDto dto, CancellationToken ct = default)
    {
        var category = await repositorio.ObtenerPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No existe la categoría {id}.");

        var nombre = dto.Name.Trim();
        if (await repositorio.ExisteNombreAsync(nombre, id, ct))
        {
            throw new InvalidOperationException($"Ya existe una categoría llamada '{nombre}'.");
        }

        category.Actualizar(nombre, dto.Description);
        await repositorio.ActualizarAsync(category, ct);
    }
}
