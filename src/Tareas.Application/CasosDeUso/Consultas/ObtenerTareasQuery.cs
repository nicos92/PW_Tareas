using Tareas.Application.Abstractions;
using Tareas.Application.Dtos;

namespace Tareas.Application.CasosDeUso.Consultas;

/// <summary>
/// Pagina y filtra las tareas. Absorbe <c>TareaState.CargarPaginaAsync</c>.
/// </summary>
public sealed class ObtenerTareasQuery(ITareaRepository repositorio)
{
    public async Task<PaginadoResult<TareaDto>> EjecutarAsync(
        ParametrosPaginacion paginacion,
        FiltroTareas filtro,
        CancellationToken ct = default)
    {
        var resultado = await repositorio.ObtenerPaginadoAsync(paginacion, filtro, ct);

        var items = resultado.Items
            .Select(TareaDto.DesdeDominio)
            .ToList()
            .AsReadOnly();

        return new PaginadoResult<TareaDto>(items, resultado.TotalCount, resultado.Page, resultado.PageSize);
    }
}
