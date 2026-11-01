using Microsoft.EntityFrameworkCore;

using Tareas.Application.Abstractions;
using Tareas.Domain.Entidades;
using Tareas.Infrastructure.Persistence;

namespace Tareas.Infrastructure.Repositories;

/// <summary>
/// Implementación EF Core del puerto <see cref="ICategoryRepository"/>.
/// </summary>
public sealed class CategoryRepository : ICategoryRepository
{
    private readonly TareaDbContext _contexto;

    public CategoryRepository(TareaDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IReadOnlyList<Category>> ObtenerTodasAsync(CancellationToken ct = default)
    {
        return await _contexto.Categorias
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public Task<Category?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        return _contexto.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public Task<bool> ExisteAsync(int id, CancellationToken ct = default)
        => _contexto.Categorias.AnyAsync(c => c.Id == id, ct);

    public Task<bool> ExisteNombreAsync(
        string nombre,
        int? excluirId = null,
        CancellationToken ct = default)
    {
        var normalizado = nombre.Trim().ToUpper();
        return _contexto.Categorias.AnyAsync(
            c => c.Name.ToUpper() == normalizado && (excluirId == null || c.Id != excluirId),
            ct);
    }

    public async Task AgregarAsync(Category category, CancellationToken ct = default)
    {
        await _contexto.Categorias.AddAsync(category, ct);
        await _contexto.SaveChangesAsync(ct);
    }

    public async Task ActualizarAsync(Category category, CancellationToken ct = default)
    {
        var previa = _contexto.Categorias.Local.FirstOrDefault(c => c.Id == category.Id);
        if (previa is not null)
        {
            _contexto.Entry(previa).State = EntityState.Detached;
        }

        _contexto.Entry(category).State = EntityState.Modified;
        await _contexto.SaveChangesAsync(ct);
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var category = await _contexto.Categorias.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (category is null)
        {
            return;
        }

        _contexto.Categorias.Remove(category);
        await _contexto.SaveChangesAsync(ct);
    }
}
