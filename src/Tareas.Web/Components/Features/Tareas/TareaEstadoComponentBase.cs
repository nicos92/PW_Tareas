using Microsoft.AspNetCore.Components;

namespace TareasBlazor.Components.Features.Tareas;

/// <summary>
/// Base de los componentes que observan <see cref="TareaEstado"/>. Centraliza la
/// suscripción a <c>OnChange</c> y su baja en <see cref="Dispose"/>, eliminando la
/// duplicación de <c>+= StateHasChanged</c> / <c>-= StateHasChanged</c> repartida por la UI.
/// </summary>
public abstract class TareaEstadoComponentBase : ComponentBase, IDisposable
{
    [Inject]
    protected TareaEstado TareaEstado { get; set; } = default!;

    protected bool EstadoInicializado { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        TareaEstado.OnChange += StateHasChanged;
        await TareaEstado.Inicializar();
        EstadoInicializado = true;
    }

    public virtual void Dispose()
    {
        TareaEstado.OnChange -= StateHasChanged;
    }
}
