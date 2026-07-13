using Microsoft.EntityFrameworkCore;

using TareasBlazor.Infraestructure.Database;
using TareasBlazor.Infraestructure.Interfaces;
using TareasBlazor.Models;

namespace TareasBlazor.Infraestructure.Repositories
{
    public class TareaSqliteRepository : ITareaRepository
    {
        private readonly AppDbContext _context;

        public TareaSqliteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TareaModel>> GetTareasAsync()
        {
            return await _context.Tareas.ToListAsync();
        }

        public async Task<TareaModel?> GetTareaByIdAsync(string idPublic)
        {
            return await _context.Tareas.FirstOrDefaultAsync(t => t.IdPublic == idPublic);
        }

        public async Task AddTareaAsync(TareaModel tarea)
        {
            // Asegurar que IdPublic sea único
            if (string.IsNullOrEmpty(tarea.IdPublic))
                tarea.IdPublic = Guid.NewGuid().ToString();

            _context.Tareas.Add(tarea);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTareaAsync(TareaModel tarea)
        {
            _context.Tareas.Update(tarea);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTareaByIdAsync(string idPublic)
        {
            var tarea = await _context.Tareas.FirstOrDefaultAsync(t => t.IdPublic == idPublic);
            if (tarea != null)
            {
                _context.Tareas.Remove(tarea);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ToggleTareaCompletadaAsync(string idPublic, bool completada)
        {
            var tarea = await _context.Tareas.FirstOrDefaultAsync(t => t.IdPublic == idPublic);
            if (tarea != null)
            {
                tarea.Completada = completada;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<TareaModel>> GetTareasCompletadasAsync()
        {
            return await _context.Tareas
                .Where(t => t.Completada)
                .ToListAsync();
        }

        public async Task<List<TareaModel>> GetTareasPendientesAsync()
        {
            return await _context.Tareas
                .Where(t => !t.Completada)
                .ToListAsync();
        }

        public async Task DeleteTareasAsync()
        {
            _context.Tareas.RemoveRange(_context.Tareas);
            await _context.SaveChangesAsync();
        }

        public async Task<PaginatedResult<TareaModel>> GetTareasPaginadasAsync(
            PaginationParams paginationParams,
            string? prioridad = null,
            string? estado = null,
            string? vencimiento = null)
        {
            IQueryable<TareaModel> query = _context.Tareas;

            if (!string.IsNullOrEmpty(prioridad) && Enum.TryParse<Prioridad>(prioridad, out var p))
                query = query.Where(t => t.Prioridad == p);

            if (!string.IsNullOrEmpty(estado))
                query = estado == "Completadas"
                    ? query.Where(t => t.Completada)
                    : query.Where(t => !t.Completada);

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

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.Id)
                .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PaginatedResult<TareaModel>(
                items.AsReadOnly(), totalCount,
                paginationParams.Page, paginationParams.PageSize);
        }

        public async Task<EstadisticasTareas> GetEstadisticasAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var tareas = await _context.Tareas.ToListAsync();

            return new EstadisticasTareas
            {
                Total = tareas.Count,
                Completadas = tareas.Count(t => t.Completada),
                Baja = tareas.Count(t => t.Prioridad == Prioridad.Baja),
                Media = tareas.Count(t => t.Prioridad == Prioridad.Media),
                Alta = tareas.Count(t => t.Prioridad == Prioridad.Alta),
                Vencidas = tareas.Count(t => t.FechaVencimiento < today),
                VencenHoy = tareas.Count(t => t.FechaVencimiento == today),
                ATiempo = tareas.Count(t => t.FechaVencimiento > today)
            };
        }
    }
}