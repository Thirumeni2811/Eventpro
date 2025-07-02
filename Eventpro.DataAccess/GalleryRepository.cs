using Eventpro.Domain.Interfaces.IGallery;
using Eventpro.Domain.Models;
using Eventpro.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Eventpro.DataAccess.Repositories
{
    public class GalleryRepository : IGalleryRepository
    {
        private readonly AppDbContext _context;

        public GalleryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Gallery>> GetAllAsync()
        {
            return await _context.Gallery.ToListAsync();
        }

        public async Task<Gallery?> GetByIdAsync(Guid id)
        {
            return await _context.Gallery.FindAsync(id);
        }

        public async Task AddAsync(Gallery gallery)
        {
            await _context.Gallery.AddAsync(gallery);
        }

        public Task UpdateAsync(Gallery gallery)
        {
            _context.Gallery.Update(gallery);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Gallery gallery)
        {
            _context.Gallery.Remove(gallery);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
