using Microsoft.EntityFrameworkCore;

using Tareas.Domain.Entidades;

namespace Tareas.Infrastructure.Persistence;

/// <summary>
/// Contexto de persistencia de tareas. Es el único dueño del esquema: las migraciones y el
/// snapshot viven junto a este tipo, en este proyecto.
/// </summary>
public class TareaDbContext : DbContext
{
    public DbSet<Tarea> Tareas => Set<Tarea>();

    public TareaDbContext(DbContextOptions<TareaDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TareaDbContext).Assembly);
    }
}
