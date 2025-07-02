using Eventpro.Domain.Models;

namespace Eventpro.Domain.Interfaces.IGallery
{
    public interface IGalleryRepository
    {
        Task<IEnumerable<Gallery>> GetAllAsync();
        Task<Gallery?> GetByIdAsync(Guid id);
        Task AddAsync(Gallery gallery);
        Task UpdateAsync(Gallery gallery);
        Task DeleteAsync(Gallery gallery);
        Task SaveChangesAsync();
    }
}
