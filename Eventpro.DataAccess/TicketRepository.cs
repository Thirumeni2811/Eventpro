using Eventpro.Domain.Interfaces.ITicket;
using Eventpro.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Eventpro.DataAccess
{
    public class TicketRepository : ITicketRepository
    {
        private readonly AppDbContext _context;

        public TicketRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tickets>> GetAllAsync()
        {
            return await _context.Tickets
                .Include(t => t.Event)
                .Include(t => t.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tickets>> GetFilteredAsync(
            Guid? ticketId,
            Guid? eventId,
            string? eventName,
            string? organizerName,
            string? buyerName)
        {
            var query = _context.Tickets
                .Include(t => t.Event)
                    .ThenInclude(e => e.User)
                .Include(t => t.User)
                .AsQueryable();

            if (ticketId.HasValue)
                query = query.Where(t => t.Id == ticketId.Value);

            if (eventId.HasValue)
                query = query.Where(t => t.EventId == eventId.Value);

            if (!string.IsNullOrWhiteSpace(eventName))
                query = query.Where(t => t.Event != null && t.Event.Name.Contains(eventName.Trim(), StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(organizerName))
                query = query.Where(t => t.Event != null && t.Event.User != null && t.Event.User.Name.Contains(organizerName.Trim(), StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(buyerName))
                query = query.Where(t => t.User != null && t.User.Name.Contains(buyerName.Trim(), StringComparison.OrdinalIgnoreCase));

            return await query
                .OrderByDescending(t => t.PurchaseDate)
                .ToListAsync();
        }


        public async Task<IEnumerable<Tickets>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Tickets
                .Include(t => t.Event)
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        public async Task<int> GetCountByUserIdAsync(Guid userId)
        {
            return await _context.Tickets
                .Where(t => t.UserId == userId)
                .SumAsync(t => t.Quantity);
        }

        public async Task<IEnumerable<Tickets>> GetByEventIdAsync(Guid eventId)
        {
            return await _context.Tickets
                .Include(t => t.User)
                .Where(t => t.EventId == eventId)
                .ToListAsync();
        }

        public async Task<IEnumerable<(string Type, int Quantity)>> GetTicketTypeCountsByEventIdAsync(Guid eventId)
        {
            return await _context.Tickets
                .Where(t => t.EventId == eventId)
                .GroupBy(t => t.Type)
                .Select(g => new ValueTuple<string, int>(g.Key, g.Sum(t => t.Quantity)))
                .ToListAsync();
        }
    }
}
