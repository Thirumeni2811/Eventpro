using Eventpro.Domain.Models;
using Eventpro.Domain.ResponseFormat;

namespace Eventpro.Domain.Interfaces.IProvide
{
    public interface IProvideService
    {
        Task<IServiceResponse<IEnumerable<Provides>>> GetAllAsync(string? titleFilter = null);
        Task<IServiceResponse<Provides?>> GetByIdAsync(Guid id);
        Task<IServiceResponse<Provides>> CreateAsync(Provides prov, string actingRole);
        Task<IServiceResponse<Provides>> UpdateAsync(Provides prov, string actingRole);
        Task<IServiceResponse<bool>> DeleteAsync(Guid id);
    }
}
