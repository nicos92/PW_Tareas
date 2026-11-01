using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Tareas.Domain.Entidades;
using Tareas.Domain.Reglas;

namespace Tareas.Infrastructure.Persistence.Configuraciones;

/// <summary>
/// Mapeo 100% Fluent de <see cref="Tarea"/>. La categoría es opcional y se vincula por FK
/// con <see cref="Category"/>; al eliminar la categoría las tareas quedan sin categoría.
/// </summary>
public sealed class TareaEntityTypeConfiguration : IEntityTypeConfiguration<Tarea>
{
    public void Configure(EntityTypeBuilder<Tarea> entity)
    {
        entity.ToTable("Tareas");

        entity.Property(t => t.Id)
            .ValueGeneratedOnAdd();

        entity.Property(t => t.IdPublic)
            .HasConversion<string>();
        entity.HasIndex(t => t.IdPublic)
            .IsUnique();

        entity.Property(t => t.Titulo)
            .HasMaxLength(TareaReglas.LongitudTituloMaxima);

        entity.Property(t => t.Descripcion)
            .HasMaxLength(TareaReglas.LongitudDescripcionMaxima);

        entity.Property(t => t.FechaVencimiento)
            .HasConversion(
                fecha => fecha.ToDateTime(TimeOnly.MinValue),
                fecha => DateOnly.FromDateTime(fecha));

        entity.Property(t => t.Prioridad)
            .HasConversion<string>();

        entity.HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasIndex(t => t.CategoryId);

        entity.Property(t => t.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        entity.HasQueryFilter(t => !t.IsDeleted);
    }
}
