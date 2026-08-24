using TareasBlazor.Infraestructure.Interfaces;
using TareasBlazor.Models;

namespace TareasBlazor.Shared
{
    public class TareaState(IWebHostEnvironment _env, ITareaRepository _repo)
    {
        private readonly SemaphoreSlim _initLock = new(1, 1);
        private bool _inicializado = false;

        public PaginatedResult<TareaModel>? ResultadoPaginado { get; private set; }
        public EstadisticasTareas Estadisticas { get; private set; } = new();
        public PaginationParams PaginacionActual { get; } = new();
        public string? FiltroPrioridad { get; private set; }
        public string? FiltroEstado { get; private set; }
        public string? FiltroVencimiento { get; private set; }
        public bool IsLoading { get; private set; }
        public event Action? OnChange;

        public async Task Inicializar()
        {
            if (_inicializado) return;

            await _initLock.WaitAsync();
            try
            {
                if (_inicializado) return;
                _inicializado = true;
            }
            finally
            {
                _initLock.Release();
            }

            await CargarPaginaAsync();
        }

        public async Task CargarPaginaAsync()
        {
            IsLoading = true;
            NotificarCambio();

            var tareaPagina = _repo.GetTareasPaginadasAsync(
                PaginacionActual, FiltroPrioridad, FiltroEstado, FiltroVencimiento);
            var estadisticas = _repo.GetEstadisticasAsync();

            await Task.WhenAll(tareaPagina, estadisticas);

            ResultadoPaginado = await tareaPagina;
            Estadisticas = await estadisticas;

            IsLoading = false;
            NotificarCambio();
        }

        public async Task SetFiltroPrioridadAsync(string? prioridad)
        {
            FiltroPrioridad = prioridad;
            PaginacionActual.Page = 1;
            await CargarPaginaAsync();
        }

        public async Task SetFiltroEstadoAsync(string? estado)
        {
            FiltroEstado = estado;
            PaginacionActual.Page = 1;
            await CargarPaginaAsync();
        }

        public async Task SetFiltroVencimientoAsync(string? vencimiento)
        {
            FiltroVencimiento = vencimiento;
            PaginacionActual.Page = 1;
            await CargarPaginaAsync();
        }

        public async Task IrAPaginaAsync(int page)
        {
            PaginacionActual.Page = page;
            await CargarPaginaAsync();
        }

        public async Task AgregarTarea(TareaModel t)
        {
            await _repo.AddTareaAsync(t);
            await CargarPaginaAsync();
        }

        public async Task EliminarTarea(string id)
        {
            var tarea = ResultadoPaginado?.Items.FirstOrDefault(t => t.IdPublic == id);
            if (tarea is not null && !string.IsNullOrEmpty(tarea.Imagen))
            {
                EliminarArchivo(tarea.Imagen);
            }

            await _repo.DeleteTareaByIdAsync(id);

            if (ResultadoPaginado is not null && ResultadoPaginado.Items.Count == 1 && PaginacionActual.Page > 1)
                PaginacionActual.Page--;

            await CargarPaginaAsync();
        }

        private void EliminarArchivo(string rutaRelativa)
        {
            if (string.IsNullOrEmpty(rutaRelativa)) return;

            var fullPath = Path.Combine(_env.WebRootPath, rutaRelativa.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        public async Task Toggle(string id)
        {
            var tarea = ResultadoPaginado?.Items.FirstOrDefault(t => t.IdPublic == id);
            if (tarea is null) return;

            await _repo.ToggleTareaCompletadaAsync(id, !tarea.Completada);
            await CargarPaginaAsync();
        }

        public async Task<TareaModel?> GetTareaById(string id)
        {
            return await _repo.GetTareaByIdAsync(id);
        }

        public async Task ActualizarTarea(TareaModel tarea)
        {
            await _repo.UpdateTareaAsync(tarea);
            await CargarPaginaAsync();
        }

        private void NotificarCambio()
        {
            OnChange?.Invoke();
        }
    }
}
