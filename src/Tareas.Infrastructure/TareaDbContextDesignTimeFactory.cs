using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

using Tareas.Infrastructure.Persistence;

namespace Tareas.Infrastructure;

/// <summary>
/// Permite que <c>dotnet ef</c> construya el contexto sin ejecutar la aplicación, cuyo
/// <c>Program.cs</c> arranca el servidor y aplica migraciones.
/// </summary>
public sealed class TareaDbContextDesignTimeFactory : IDesignTimeDbContextFactory<TareaDbContext>
{
    public TareaDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile(Path.Combine("src", "Tareas.Web", "appsettings.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cadena = RutaBaseDatos.ResolverConexion(
            configuration.GetConnectionString("Tareas"),
            configuration["DATA_DIR"]);

        var opciones = new DbContextOptionsBuilder<TareaDbContext>()
            .UseSqlite(cadena)
            .Options;

        return new TareaDbContext(opciones);
    }
}
