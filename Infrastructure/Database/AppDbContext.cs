using Microsoft.EntityFrameworkCore;

using TareasBlazor.Models;

namespace TareasBlazor.Infraestructure.Database
{
    public class AppDbContext : DbContext
    {
        public DbSet<TareaModel> Tareas => Set<TareaModel>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TareaModel>(entity =>
            {
                // Configurar Id como autoincremental
                entity.Property(t => t.Id)
                      .ValueGeneratedOnAdd();

                // Configurar IdPublic como único
                entity.HasIndex(t => t.IdPublic)
                      .IsUnique();

                // Configurar Prioridad enum como string
                entity.Property(t => t.Prioridad)
                      .HasConversion<string>();

                // Configurar DateOnly (EF Core 8 soporta nativamente)
                entity.Property(t => t.FechaVencimiento)
                      .HasConversion(
                          d => d.HasValue ? d.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                          d => d.HasValue ? DateOnly.FromDateTime(d.Value) : (DateOnly?)null);
            });
        }
    }
}