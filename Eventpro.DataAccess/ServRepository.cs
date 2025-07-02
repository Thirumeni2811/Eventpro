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
            return await _context.Services.ToListAsync();
        }

        public async Task<Services?> GetByIdAsync(Guid id)
        {
            return await _context.Services.FindAsync(id);
        }

        public async Task AddAsync(Services serv)
        {
            await _context.Services.AddAsync(serv);
        }

        public Task UpdateAsync(Services serv)
        {
            _context.Services.Update(serv);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Services serv)
        {
            _context.Services.Remove(serv);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
