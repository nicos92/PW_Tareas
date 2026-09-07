
using Microsoft.EntityFrameworkCore;

using TareasBlazor.Components;
using TareasBlazor.Infraestructure.Database;
using TareasBlazor.Infraestructure.Interfaces;
using TareasBlazor.Infraestructure.Repositories;
using TareasBlazor.Shared;

// Comentario para la simulación de un pull request
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 1. Obtener la ruta de la carpeta Local del usuario (ej: C:\Users\Usuario\AppData\Local)
var localDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

// 2. (Opcional pero recomendado) Crear una subcarpeta para tu aplicación para no ensuciar la raíz
var appDataPath = Path.Combine(localDataPath, "Nicolas_Sandoval", "PW_Tareas");


// Asegurarse de que la carpeta exista
Directory.CreateDirectory(appDataPath);

// 3. Definir la ruta completa del archivo .db
var dbPath = Path.Combine(appDataPath, "tareas.db");
// Configurar DbContext con SQLite
// var dbPath = Path.Combine(builder.Environment.ContentRootPath, "tareas.db");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));
builder.Services.AddScoped<ThemeService>();


// Registrar repositorio (elegir una implementación)
builder.Services.AddScoped<ITareaRepository, TareaSqliteRepository>();

// Registrar estado de tareas
builder.Services.AddScoped<TareaState>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();