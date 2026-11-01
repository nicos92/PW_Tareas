using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

using Tareas.Application.Dtos;
using TareasBlazor.Components.Features.Tareas;

namespace TareasBlazor.Components.Pages;

public partial class ListaTareas : TareaEstadoComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private static readonly string[] _columnasTabla = { "Lista", "Título", "Categoría", "Prioridad", "Acciones" };

    private bool _showBajaModal;
    private TareaDto? _tareaADarDeBaja;

    private Task OnPrioridadChanged(string valor) => TareaEstado.SetFiltroPrioridadAsync(valor);

    private Task OnEstadoChanged(string valor) => TareaEstado.SetFiltroEstadoAsync(valor);

    private Task OnVencimientoChanged(string valor) => TareaEstado.SetFiltroVencimientoAsync(valor);

    private Task OnBajaChanged(string valor) => TareaEstado.SetFiltroBajaAsync(valor);

    private Task OnPageChanged(int page) => TareaEstado.IrAPaginaAsync(page);

    private void VerTarea(TareaDto tarea)
    {
        NavigationManager.NavigateTo($"/listaDeTareas/tarea/{tarea.IdPublic}");
    }

    private Task CompletarTarea(TareaDto tarea) => TareaEstado.ToggleAsync(tarea.IdPublic);

    private Task RestaurarTarea(TareaDto tarea) => TareaEstado.RestaurarTareaAsync(tarea.IdPublic);

    private void DarDeBajaTarea(TareaDto tarea)
    {
        _tareaADarDeBaja = tarea;
        _showBajaModal = true;
    }

    private async Task ConfirmarBaja()
    {
        if (_tareaADarDeBaja is not null)
        {
            await TareaEstado.DarDeBajaTareaAsync(_tareaADarDeBaja.IdPublic);
            _tareaADarDeBaja = null;
        }

        _showBajaModal = false;
    }

    private void CancelarBaja()
    {
        _tareaADarDeBaja = null;
        _showBajaModal = false;
    }

    private void IniciarEdicion(string id)
    {
        NavigationManager.NavigateTo($"/listaDeTareas/editarTarea/{id}");
    }

    private void NuevaTarea()
    {
        NavigationManager.NavigateTo("/listaDeTareas/nuevaTarea");
    }
}
