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
    }
}