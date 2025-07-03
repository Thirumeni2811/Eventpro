using Eventpro.Domain.Exceptions;
using Eventpro.Domain.Interfaces.IGallery;
using Eventpro.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Eventpro.DataAccess
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
            try
            {
                return await _context.Gallery.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error retrieving all galleries.", ex);
            }
        }

        public async Task<Gallery?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Gallery.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error retrieving gallery with ID {id}.", ex);
            }
        }

        public async Task AddAsync(Gallery gallery)
        {
            try
            {
                await _context.Gallery.AddAsync(gallery);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error adding new gallery.", ex);
            }
        }

        public Task UpdateAsync(Gallery gallery)
        {
            try
            {
                _context.Gallery.Update(gallery);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error updating gallery with ID {gallery?.Id}.", ex);
            }
        }

        public Task DeleteAsync(Gallery gallery)
        {
            try
            {
                _context.Gallery.Remove(gallery);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error deleting gallery with ID {gallery?.Id}.", ex);
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error saving changes to the database.", ex);
            }
        }
    }
}
