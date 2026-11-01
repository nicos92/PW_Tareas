using Tareas.Application.Abstractions;
using Tareas.Application.Dtos;

namespace Tareas.Application.CasosDeUso.Consultas;

/// <summary>
/// Devuelve todas las categorías ordenadas por nombre para alimentar el listado y el
/// selector de categorías de los formularios de tareas.
/// </summary>
public sealed class GetCategoriesQuery(ICategoryRepository repositorio)
{
    public async Task<IReadOnlyList<CategoryDto>> EjecutarAsync(CancellationToken ct = default)
    {
        var categorias = await repositorio.ObtenerTodasAsync(ct);

        return categorias
            .Select(CategoryDto.DesdeDominio)
            .ToList()
            .AsReadOnly();
    }
}
