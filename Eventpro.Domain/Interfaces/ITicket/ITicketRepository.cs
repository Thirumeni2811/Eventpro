    using Eventpro.Domain.Models;

    namespace Eventpro.Domain.Interfaces.ITicket
    {
        public interface ITicketRepository
        {
            Task<IEnumerable<Tickets>> GetAllAsync();
            Task<IEnumerable<Tickets>> GetFilteredAsync(
                Guid? ticketId,
                Guid? eventId,
                string? eventName,
                string? organizerName,
                string? buyerName);

            Task<IEnumerable<Tickets>> GetByUserIdAsync(Guid userId);
            Task<int> GetCountByUserIdAsync(Guid userId);
            Task<IEnumerable<Tickets>> GetByEventIdAsync(Guid eventId);
            Task<IEnumerable<(string Type, int Quantity)>> GetTicketTypeCountsByEventIdAsync(Guid eventId);
        }
    }
