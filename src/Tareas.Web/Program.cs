using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

using System.Globalization;

using Tareas.Application;
using Tareas.Application.Configuracion;
using Tareas.Infrastructure;
using Tareas.Infrastructure.Persistence;
using TareasBlazor.Components;
using TareasBlazor.Components.Features.Categorias;
using TareasBlazor.Components.Features.Tareas;
using TareasBlazor.Components.Features.Tema;
using TareasBlazor.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.WebRootPath);

builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<TareaEstado>();
builder.Services.AddScoped<CategoriaEstado>();

var app = builder.Build();

var culture = new CultureInfo("es-AR");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = [culture],
    SupportedUICultures = [culture],
    ApplyCurrentCultureToResponseHeaders = true
};
app.UseRequestLocalization(localizationOptions);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseArchivosEstaticos(
    builder.Configuration["Storage:UploadsPhysicalPath"],
    builder.Configuration["Storage:UploadsPath"] ?? OpcionesAlmacenamiento.RutaUploadsPorDefecto);
app.UseAntiforgery();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/health/ready", async (IDbContextFactory<TareaDbContext> fabrica) =>
{
    await using var db = await fabrica.CreateDbContextAsync();
    return await db.Database.CanConnectAsync()
        ? Results.Ok(new { status = "ok" })
        : Results.Json(
            new { status = "unavailable" },
            statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var fabrica = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TareaDbContext>>();
        await using var db = await fabrica.CreateDbContextAsync();
        await db.Database.MigrateAsync();

        await SeedData.InicializarAsync(db);

        var ruta = db.Database.GetDbConnection().DataSource;
        logger.LogInformation("Base de datos SQLite en {Ruta}", ruta);

        var dataDir = Environment.GetEnvironmentVariable("DATA_DIR");
        if (!string.IsNullOrWhiteSpace(dataDir) && !ruta.StartsWith(dataDir, StringComparison.Ordinal))
        {
            logger.LogWarning(
                "El directorio {Directorio} no admite escritura: la base se guardó en {Respaldo}. " +
                "En Cloud Run usá DATA_DIR=/tmp/data.",
                dataDir,
                ruta);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Fallo al aplicar las migraciones de la base de datos.");
        throw;
    }
}

app.Run();
