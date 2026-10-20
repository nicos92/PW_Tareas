using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

using System.Globalization;

using Tareas.Application;
using Tareas.Infrastructure;
using Tareas.Infrastructure.Persistence;
using TareasBlazor.Components;
using TareasBlazor.Components.Features.Tareas;
using TareasBlazor.Components.Features.Tema;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.WebRootPath);

builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<TareaEstado>();

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

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<TareaDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Fallo al aplicar las migraciones de la base de datos.");
        throw;
    }
}

app.Run();
