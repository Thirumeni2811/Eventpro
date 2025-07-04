using Eventpro.Domain.Models;
using Eventpro.Domain.ResponseFormat;

namespace Eventpro.Domain.Interfaces.IEvents
{
    public interface IEventService
    {
        Task<IServiceResponse<IEnumerable<Events>>> GetAllEventsAsync(
            string actingRole,
            Guid? eventId = null,
            string? name = null,
            string? organizedBy = null,
            string? type = null,
            string? venue = null,
            string? status = null,
            string? isPaid = null
        );

        Task<IServiceResponse<IEnumerable<Events>>> GetPublicEventsAsync(
            string name,
            string status,
            string type,
            string venue
        );

        Task<IServiceResponse<Events>> GetEventByIdAsync(Guid id);

        Task<IServiceResponse<IEnumerable<Events>>> GetEventsByUserIdAsync(
            Guid userId,
            string eventName,
            string status
        );

        Task<IServiceResponse<Events>> CreateEventAsync(
            Events evt,
            string actingRole,
            Guid actingUserId
        );

        Task<IServiceResponse<Events>> UpdateEventAsync(
            Events evt,
            string actingRole,
            Guid actingUserId
        );

        Task<IServiceResponse<bool>> DeleteEventAsync(
            Guid id,
            string actingRole,
            Guid actingUserId
        );
        Task<IServiceResponse<IEnumerable<Events>>> GetEventsAsync(string actingRole);
    }
}
