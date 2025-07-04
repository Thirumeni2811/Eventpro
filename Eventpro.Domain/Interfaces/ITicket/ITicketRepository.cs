    using Eventpro.Domain.Models;

    namespace Eventpro.Domain.Interfaces.ITicket
    {
        public interface ITicketRepository
        {
            Task<IEnumerable<Tickets>> GetAllAsync();
            Task<IEnumerable<Tickets>> GetAllTicketsAsync(
                Guid? ticketId,
                Guid? eventId,
                string? eventName,
                string? organizerName,
                string? buyerName);
            Task<IEnumerable<Tickets>> GetByUserIdAsync(Guid userId);
            Task<int> GetCountByUserIdAsync(Guid userId);
            Task<int> GetBookedQuantityByEventIdAsync(Guid eventId);
            Task<IEnumerable<Tickets>> GetByEventIdAsync(Guid eventId);
            Task<IEnumerable<(string Type, int Quantity)>> GetTicketTypeCountsByEventIdAsync(Guid eventId);
            Task<IEnumerable<Events>> GetDistinctEventsByUserIdAsync(Guid userId);
            Task AddRangeAsync(IEnumerable<Tickets> tickets);
            Task SaveChangesAsync();


    }
}
