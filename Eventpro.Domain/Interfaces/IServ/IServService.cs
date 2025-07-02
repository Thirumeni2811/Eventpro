using Eventpro.Domain.Models;
using Eventpro.Domain.ResponseFormat;

namespace Eventpro.Domain.Interfaces.IServ
{
    public interface IServService
    {
        Task<IServiceResponse<IEnumerable<Services>>> GetAllAsync(string? titleFilter = null);
        Task<IServiceResponse<Services?>> GetByIdAsync(Guid id);
        Task<IServiceResponse<Services>> CreateAsync(Services serv, string actingRole);
        Task<IServiceResponse<Services>> UpdateAsync(Services serv, string actingRole);
        Task<IServiceResponse<bool>> DeleteAsync(Guid id);
    }
}
