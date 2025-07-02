using Eventpro.Domain.Models;

namespace Eventpro.Domain.Interfaces.IProvide
{
    public interface IProvideRepository
    {
        Task<IEnumerable<Provides>> GetAllAsync();
        Task<Provides?> GetByIdAsync(Guid id);
        Task AddAsync(Provides prov);
        Task UpdateAsync(Provides prov);
        Task DeleteAsync(Provides prov);
        Task SaveChangesAsync();
    }
}
