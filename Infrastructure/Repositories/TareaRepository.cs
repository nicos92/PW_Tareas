using TareasBlazor.Infraestructure.Database;
using TareasBlazor.Infraestructure.Interfaces;
using TareasBlazor.Models;

namespace TareasBlazor.Infraestructure.Repositories
{
    public class TareaRepository : ITareaRepository
    {
        public async Task<List<TareaModel>> GetTareasAsync()
        {
            return await Task.FromResult(InMemory.Tareas);
        }

        public async Task AddTareaAsync(TareaModel tarea)
        {
            InMemory.Tareas.Add(tarea);
            await Task.CompletedTask;
        }

        public async Task DeleteTareasAsync()
        {
            InMemory.Tareas.Clear();
            await Task.CompletedTask;
        }

        public async Task UpdateTareaAsync(TareaModel tarea)
        {
            var existingTarea = InMemory.Tareas.FirstOrDefault(t => t.IdPublic == tarea.IdPublic);
            if (existingTarea != null)
            {
                existingTarea.Titulo = tarea.Titulo;
                existingTarea.Completada = tarea.Completada;
            }
            await Task.CompletedTask;
        }

        public async Task<TareaModel?> GetTareaByIdAsync(string id)
        {
            var tarea = InMemory.Tareas.FirstOrDefault(t => t.IdPublic == id);
            return await Task.FromResult(tarea);
        }

        public async Task DeleteTareaByIdAsync(string id)
        {
            var tarea = InMemory.Tareas.FirstOrDefault(t => t.IdPublic == id);
            if (tarea != null)
            {
                InMemory.Tareas.Remove(tarea);
            }
            await Task.CompletedTask;
        }

        public async Task ToggleTareaCompletadaAsync(string id, bool completada)
        {
            var tarea = InMemory.Tareas.FirstOrDefault(t => t.IdPublic == id);
            if (tarea != null)
            {
                tarea.Completada = completada;
            }
            await Task.CompletedTask;
        }

        public async Task<List<TareaModel>> GetTareasCompletadasAsync()
        {
            var tareasCompletadas = InMemory.Tareas.Where(t => t.Completada).ToList();
            return await Task.FromResult(tareasCompletadas);
        }

        public async Task<List<TareaModel>> GetTareasPendientesAsync()
        {
            var tareasPendientes = InMemory.Tareas.Where(t => !t.Completada).ToList();
            return await Task.FromResult(tareasPendientes);
        }
    }
}