using Tareas.Application.Abstractions;

namespace Tareas.Application.CasosDeUso.Comandos;

/// <summary>
/// Elimina una categoría sólo si no tiene tareas asociadas. La restricción también está
/// reforzada en la base con <c>ON DELETE RESTRICT</c>.
/// </summary>
public sealed class DeleteCategoryCommand(ICategoryRepository repositorio)
{
    public async Task EjecutarAsync(int id, CancellationToken ct = default)
    {
        if (await repositorio.TieneTareasAsync(id, ct))
        {
            throw new InvalidOperationException(
                "No se puede eliminar la categoría porque tiene tareas asociadas.");
        }

        await repositorio.EliminarAsync(id, ct);
    }
}
