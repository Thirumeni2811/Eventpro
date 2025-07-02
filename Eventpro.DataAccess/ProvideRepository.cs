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
            return await _context.Provides.ToListAsync();
        }

        public async Task<Provides?> GetByIdAsync(Guid id)
        {
            return await _context.Provides.FindAsync(id);
        }

        public async Task AddAsync(Provides prov)
        {
            await _context.Provides.AddAsync(prov);
        }

        public Task UpdateAsync(Provides prov)
        {
            _context.Provides.Update(prov);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Provides prov)
        {
            _context.Provides.Remove(prov);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
