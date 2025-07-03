using Eventpro.Domain.Exceptions;
using Eventpro.Domain.Interfaces.IServ;
using Eventpro.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Eventpro.DataAccess
{
    public class ServRepository : IServRepository
    {
        private readonly AppDbContext _context;

        public ServRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Services>> GetAllAsync()
        {
            try
            {
                return await _context.Services.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error retrieving all services.", ex);
            }
        }

        public async Task<Services?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Services.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error retrieving service with ID {id}.", ex);
            }
        }

        public async Task AddAsync(Services serv)
        {
            try
            {
                await _context.Services.AddAsync(serv);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error adding new service.", ex);
            }
        }

        public Task UpdateAsync(Services serv)
        {
            try
            {
                _context.Services.Update(serv);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error updating service with ID {serv?.Id}.", ex);
            }
        }

        public Task DeleteAsync(Services serv)
        {
            try
            {
                _context.Services.Remove(serv);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error deleting service with ID {serv?.Id}.", ex);
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
