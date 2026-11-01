using Tareas.Application.Abstractions;
using Tareas.Application.Dtos;

namespace Tareas.Application.CasosDeUso.Consultas;

public sealed class GetCategoryByIdQuery(ICategoryRepository repositorio)
{
    public async Task<CategoryDto?> EjecutarAsync(int id, CancellationToken ct = default)
    {
        var category = await repositorio.ObtenerPorIdAsync(id, ct);
        return category is null ? null : CategoryDto.DesdeDominio(category);
    }
}
