using Eventpro.Domain.Models;

namespace Eventpro.Domain.Interfaces.IServ
{
    public interface IServRepository
    {
        Task<IEnumerable<Services>> GetAllAsync();
        Task<Services?> GetByIdAsync(Guid id);
        Task AddAsync(Services serv);
        Task UpdateAsync(Services serv);
        Task DeleteAsync(Services serv);
        Task SaveChangesAsync();
    }
}
