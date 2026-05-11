using TareasBlazor.Infraestructure.Interfaces;
using TareasBlazor.Models;
using System.IO;

namespace TareasBlazor.Shared
{
    public class TareaState(IWebHostEnvironment _env, ITareaRepository _repo)
    {
        private readonly List<TareaModel> _tareas = [];
        private bool _inicializado = false;

        public IReadOnlyList<TareaModel> Tareas => _tareas.AsReadOnly();
        public event Action? OnChange;

        public async Task Inicializar()
        {
            if (_inicializado) return;

            var tareas = await _repo.GetTareasAsync();

            _tareas.Clear();
            _tareas.AddRange([.. tareas.OrderByDescending(t => t.Id)]);

            _inicializado = true;

            NotificarCambio();
        }

        public async Task AgregarTarea(TareaModel t)
        {
            _tareas.Add(t);
            Console.WriteLine(_tareas);
            await _repo.AddTareaAsync(t);
            NotificarCambio();
        }

        public async Task EliminarTarea(string id)
        {
            var tarea = _tareas.FirstOrDefault(t => t.IdPublic == id);
            if (tarea is not null && !string.IsNullOrEmpty(tarea.Imagen))
            {
                EliminarArchivo(tarea.Imagen);
            }

            _tareas.RemoveAll(t => t.IdPublic == id);
            await _repo.DeleteTareaByIdAsync(id);
            NotificarCambio();
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
            var tarea = _tareas.FirstOrDefault(t => t.IdPublic == id);
            if (tarea is null) return;

            tarea.Completada = !tarea.Completada;

            await _repo.ToggleTareaCompletadaAsync(id, tarea.Completada);
            NotificarCambio();
        }

        public IReadOnlyList<TareaModel> FiltrarPorPrioridad(string prioridad)
        {
            if (!Enum.TryParse<Prioridad>(prioridad, out var prioridadEnum))
                return _tareas.AsReadOnly();

            return _tareas.Where(t => t.Prioridad == prioridadEnum).ToList().AsReadOnly();
        }

        public IReadOnlyList<TareaModel> FiltrarPorCompletada(bool? completada)
        {
            if (completada is null)
                return _tareas.AsReadOnly();

            return _tareas.Where(t => t.Completada == completada).ToList().AsReadOnly();
        }

        public TareaModel? GetTareaById(string id)
        {
            return _tareas.FirstOrDefault(t => t.IdPublic == id);
        }

        public async Task ActualizarTarea(TareaModel tarea)
        {
            var index = _tareas.FindIndex(t => t.IdPublic == tarea.IdPublic);
            if (index >= 0)
            {
                _tareas[index] = tarea;
                await _repo.UpdateTareaAsync(tarea);
                NotificarCambio();
            }
        }

        private void NotificarCambio() => OnChange?.Invoke();
    }
}
