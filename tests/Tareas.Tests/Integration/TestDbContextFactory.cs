using Microsoft.EntityFrameworkCore;

using Tareas.Infrastructure.Persistence;

namespace Tareas.Tests.Integration;

/// <summary>
/// Factory de test que crea contextos sobre las mismas opciones y la misma conexión
/// SQLite in-memory abierta que usa cada prueba.
/// </summary>
internal sealed class TestDbContextFactory(DbContextOptions<TareaDbContext> opciones)
    : IDbContextFactory<TareaDbContext>
{
    public TareaDbContext CreateDbContext() => new(opciones);

    public Task<TareaDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(CreateDbContext());
}
