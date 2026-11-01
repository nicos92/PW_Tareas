using Tareas.Application.Abstractions;

namespace Tareas.Application.CasosDeUso.Comandos;

/// <summary>
/// Elimina una categoría. La FK de tareas es opcional y está configurada con
/// <c>ON DELETE SET NULL</c>, por lo que las tareas asociadas quedan sin categoría.
/// </summary>
public sealed class DeleteCategoryCommand(ICategoryRepository repositorio)
{
    public Task EjecutarAsync(int id, CancellationToken ct = default)
        => repositorio.EliminarAsync(id, ct);
}
