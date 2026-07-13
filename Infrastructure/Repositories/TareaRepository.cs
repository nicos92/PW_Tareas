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

        public Task<PaginatedResult<TareaModel>> GetTareasPaginadasAsync(
            PaginationParams paginationParams,
            string? prioridad = null,
            string? estado = null,
            string? vencimiento = null)
        {
            IEnumerable<TareaModel> query = InMemory.Tareas;

            if (!string.IsNullOrEmpty(prioridad) && Enum.TryParse<Prioridad>(prioridad, out var p))
                query = query.Where(t => t.Prioridad == p);

            if (!string.IsNullOrEmpty(estado))
                query = estado == "Completadas" ? query.Where(t => t.Completada) : query.Where(t => !t.Completada);

            if (!string.IsNullOrEmpty(vencimiento))
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                query = vencimiento switch
                {
                    "Vencidas" => query.Where(t => t.FechaVencimiento < today),
                    "VencenHoy" => query.Where(t => t.FechaVencimiento == today),
                    "ATiempo" => query.Where(t => t.FechaVencimiento > today),
                    _ => query
                };
            }

            var list = query.OrderByDescending(t => t.Id).ToList();
            var totalCount = list.Count;
            var items = list.Skip((paginationParams.Page - 1) * paginationParams.PageSize)
                            .Take(paginationParams.PageSize).ToList();

            return Task.FromResult(new PaginatedResult<TareaModel>(
                items.AsReadOnly(), totalCount, paginationParams.Page, paginationParams.PageSize));
        }

        public Task<EstadisticasTareas> GetEstadisticasAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            return Task.FromResult(new EstadisticasTareas
            {
                Total = InMemory.Tareas.Count,
                Completadas = InMemory.Tareas.Count(t => t.Completada),
                Baja = InMemory.Tareas.Count(t => t.Prioridad == Prioridad.Baja),
                Media = InMemory.Tareas.Count(t => t.Prioridad == Prioridad.Media),
                Alta = InMemory.Tareas.Count(t => t.Prioridad == Prioridad.Alta),
                Vencidas = InMemory.Tareas.Count(t => t.FechaVencimiento < today),
                VencenHoy = InMemory.Tareas.Count(t => t.FechaVencimiento == today),
                ATiempo = InMemory.Tareas.Count(t => t.FechaVencimiento > today)
            });
        }
    }
}