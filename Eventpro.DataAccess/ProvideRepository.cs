using Eventpro.Domain.Exceptions;
using Eventpro.Domain.Interfaces.IProvide;
using Eventpro.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Eventpro.DataAccess
{
    public class ProvideRepository : IProvideRepository
    {
        private readonly AppDbContext _context;

        public ProvideRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Provides>> GetAllAsync()
        {
            try
            {
                return await _context.Provides.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error retrieving all provides.", ex);
            }
        }

        public async Task<Provides?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Provides.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error retrieving provide with ID {id}.", ex);
            }
        }

        public async Task AddAsync(Provides prov)
        {
            try
            {
                await _context.Provides.AddAsync(prov);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error adding new provide.", ex);
            }
        }

        public Task UpdateAsync(Provides prov)
        {
            try
            {
                _context.Provides.Update(prov);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error updating provide with ID {prov?.Id}.", ex);
            }
        }

        public Task DeleteAsync(Provides prov)
        {
            try
            {
                _context.Provides.Remove(prov);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error deleting provide with ID {prov?.Id}.", ex);
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
