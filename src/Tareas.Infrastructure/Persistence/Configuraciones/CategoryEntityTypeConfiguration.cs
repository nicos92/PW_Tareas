using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Tareas.Domain.Entidades;
using Tareas.Domain.Reglas;

namespace Tareas.Infrastructure.Persistence.Configuraciones;

/// <summary>
/// Mapeo Fluent de <see cref="Category"/>. El nombre es único para evitar categorías
/// duplicadas desde la UI.
/// </summary>
public sealed class CategoryEntityTypeConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> entity)
    {
        entity.ToTable("Categorias");

        entity.Property(c => c.Id)
            .ValueGeneratedOnAdd();

        entity.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(CategoryReglas.LongitudNombreMaxima);

        entity.Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(CategoryReglas.LongitudDescripcionMaxima);

        entity.HasIndex(c => c.Name)
            .IsUnique();
    }
}
