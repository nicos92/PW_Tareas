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
    private readonly IDbContextFactory<TareaDbContext> _fabrica;

    public CategoryRepository(IDbContextFactory<TareaDbContext> fabrica)
    {
        _fabrica = fabrica;
    }

    public async Task<IReadOnlyList<Category>> ObtenerTodasAsync(CancellationToken ct = default)
    {
        await using var contexto = await _fabrica.CreateDbContextAsync(ct);

        return await contexto.Categorias
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<Category?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        await using var contexto = await _fabrica.CreateDbContextAsync(ct);

        return await contexto.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<bool> ExisteAsync(int id, CancellationToken ct = default)
    {
        await using var contexto = await _fabrica.CreateDbContextAsync(ct);

        return await contexto.Categorias.AnyAsync(c => c.Id == id, ct);
    }

    public async Task<bool> ExisteNombreAsync(
        string nombre,
        int? excluirId = null,
        CancellationToken ct = default)
    {
        await using var contexto = await _fabrica.CreateDbContextAsync(ct);

        var normalizado = nombre.Trim().ToUpper();
        return await contexto.Categorias.AnyAsync(
            c => c.Name.ToUpper() == normalizado && (excluirId == null || c.Id != excluirId),
            ct);
    }

    public async Task<bool> TieneTareasAsync(int id, CancellationToken ct = default)
    {
        await using var contexto = await _fabrica.CreateDbContextAsync(ct);

        return await contexto.Tareas
            .IgnoreQueryFilters()
            .AnyAsync(t => t.CategoryId == id, ct);
    }

    public async Task AgregarAsync(Category category, CancellationToken ct = default)
    {
        await using var contexto = await _fabrica.CreateDbContextAsync(ct);

        await contexto.Categorias.AddAsync(category, ct);
        await contexto.SaveChangesAsync(ct);
    }

    public async Task ActualizarAsync(Category category, CancellationToken ct = default)
    {
        await using var contexto = await _fabrica.CreateDbContextAsync(ct);

        contexto.Entry(category).State = EntityState.Modified;
        await contexto.SaveChangesAsync(ct);
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        await using var contexto = await _fabrica.CreateDbContextAsync(ct);

        var category = await contexto.Categorias.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (category is null)
        {
            return;
        }

        contexto.Categorias.Remove(category);
        await contexto.SaveChangesAsync(ct);
    }
}
