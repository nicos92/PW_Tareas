using Microsoft.Extensions.DependencyInjection;

using Tareas.Application.CasosDeUso.Comandos;
using Tareas.Application.CasosDeUso.Consultas;

namespace Tareas.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ObtenerTareasQuery>();
        services.AddScoped<ObtenerTareaPorIdQuery>();
        services.AddScoped<ObtenerEstadisticasQuery>();
        services.AddScoped<GetCategoriesQuery>();
        services.AddScoped<GetCategoryByIdQuery>();

        services.AddScoped<CrearTareaCommand>();
        services.AddScoped<ActualizarTareaCommand>();
        services.AddScoped<DarDeBajaTareaCommand>();
        services.AddScoped<RestaurarTareaCommand>();
        services.AddScoped<CambiarCompletadaTareaCommand>();

        services.AddScoped<CreateCategoryCommand>();
        services.AddScoped<UpdateCategoryCommand>();
        services.AddScoped<DeleteCategoryCommand>();

        return services;
    }
}
