using Microsoft.AspNetCore.Components.Forms;

using Tareas.Application.Dtos;

namespace TareasBlazor.Components.Features.Archivos;

/// <summary>
/// Adaptador entre <see cref="IBrowserFile"/> (contrato de Blazor) y
/// <see cref="SubidaArchivo"/> (contrato de Application). Mantiene el límite de subida
/// fuera de los componentes y permite que Application no dependa de Blazor.
/// </summary>
public static class SubidaArchivoFactory
{
    public static SubidaArchivo Crear(IBrowserFile archivo, long maximoBytes)
    {
        return new SubidaArchivo(archivo.OpenReadStream(maximoBytes), archivo.Name);
    }
}
