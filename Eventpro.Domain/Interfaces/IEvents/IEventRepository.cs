using Eventpro.Domain.Models;

namespace Eventpro.Domain.Interfaces.IEvents
{
    public interface IEventRepository
    {
        Task<IEnumerable<Events>> GetAllAsync();
        Task<IEnumerable<Events>> GetFilteredAsync(
            Guid? eventId = null,
            string? name = null,
            string? organizedBy = null,
            string? type = null,
            string? venue = null,
            string? status = null,
            string? isPaid = null);
        Task<IEnumerable<Events>> GetFilteredPublicEventsAsync(
            string name,
            string status,
            string type,
            string venue
        );
        Task<Events?> GetByIdAsync(Guid id);
        Task<IEnumerable<Events>> GetByUserIdAsync(Guid userId);
        Task AddAsync(Events evt);
        Task UpdateAsync(Events evt);
        Task DeleteAsync(Events evt);
        Task<IEnumerable<Events>> GetEventsAsync();
        Task SaveChangesAsync();
    }
}
