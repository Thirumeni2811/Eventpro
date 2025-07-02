using Eventpro.Domain.Models;
using Eventpro.Domain.ResponseFormat;

namespace Eventpro.Domain.Interfaces.IGallery
{
    public interface IGalleryService
    {
        Task<IServiceResponse<IEnumerable<Gallery>>> GetAllAsync(string? nameFilter = null, string? typeFilter = null);
        Task<IServiceResponse<Gallery?>> GetByIdAsync(Guid id);
        Task<IServiceResponse<Gallery>> CreateAsync(Gallery gallery, string actingRole);
        Task<IServiceResponse<Gallery>> UpdateAsync(Gallery gallery, string actingRole);
        Task<IServiceResponse<bool>> DeleteAsync(Guid id);
    }
}
