using Eventpro.Domain.Models;
using Eventpro.Domain.ResponseFormat;

namespace Eventpro.Domain.Interfaces.ITicket
{
    public interface ITicketService
    {
        Task<IServiceResponse<IEnumerable<Tickets>>> GetAllTicketsAsync(
            string actingRole,
            Guid? ticketId = null,
            Guid? eventId = null,
            string eventName = null,
            string organizerName = null,
            string buyerName = null
        );

        Task<IServiceResponse<IEnumerable<Tickets>>> GetTicketsByUserIdAsync(Guid userId, string? eventName = null, string? status = null);

        Task<IServiceResponse<int>> GetTicketCountByUserIdAsync(Guid userId, string actingRole);

        Task<IServiceResponse<IEnumerable<Tickets>>> GetTicketsByEventIdAsync(Guid eventId, string actingRole);

        Task<IServiceResponse<IEnumerable<(string Type, int Quantity)>>> GetTicketTypeCountsByEventIdAsync(Guid eventId);
    }
}
