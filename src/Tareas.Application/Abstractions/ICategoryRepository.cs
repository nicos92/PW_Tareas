using Tareas.Domain.Entidades;

namespace Tareas.Application.Abstractions;

/// <summary>
/// Puerto de persistencia de categorías. La capa de presentación sólo consume
/// <c>CategoryDto</c>; nunca ve la entidad de dominio.
/// </summary>
public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> ObtenerTodasAsync(CancellationToken ct = default);

    Task<Category?> ObtenerPorIdAsync(int id, CancellationToken ct = default);

    Task<bool> ExisteAsync(int id, CancellationToken ct = default);

    Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null, CancellationToken ct = default);

    Task<bool> TieneTareasAsync(int id, CancellationToken ct = default);

    Task AgregarAsync(Category category, CancellationToken ct = default);

    Task ActualizarAsync(Category category, CancellationToken ct = default);

    Task EliminarAsync(int id, CancellationToken ct = default);
}
