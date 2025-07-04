using Eventpro.Domain.Exceptions;
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
            try
            {
                return await _context.Tickets
                    .Include(t => t.Event)
                    .Include(t => t.User)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error retrieving all tickets.", ex);
            }
        }

        public async Task<IEnumerable<Tickets>> GetAllTicketsAsync(
            Guid? ticketId,
            Guid? eventId,
            string? eventName,
            string? organizerName,
            string? buyerName)
        {
            try
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
                {
                    var eventNameTrimmed = eventName.Trim().ToLower();
                    query = query.Where(t =>
                        t.Event != null &&
                        t.Event.Name.ToLower().Contains(eventNameTrimmed));
                }

                if (!string.IsNullOrWhiteSpace(organizerName))
                {
                    var organizerNameTrimmed = organizerName.Trim().ToLower();
                    query = query.Where(t =>
                        t.Event != null &&
                        t.Event.User != null &&
                        t.Event.User.Name.ToLower().Contains(organizerNameTrimmed));
                }

                if (!string.IsNullOrWhiteSpace(buyerName))
                {
                    var buyerNameTrimmed = buyerName.Trim().ToLower();
                    query = query.Where(t =>
                        t.User != null &&
                        t.User.Name.ToLower().Contains(buyerNameTrimmed));
                }

                return await query
                    .OrderByDescending(t => t.PurchaseDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error filtering tickets.", ex);
            }
        }

        public async Task<IEnumerable<Tickets>> GetByUserIdAsync(Guid userId)
        {
            try
            {
                return await _context.Tickets
                    .Include(t => t.Event)
                    .ThenInclude(e => e.User)
                    .Where(t => t.UserId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error retrieving tickets for user {userId}.", ex);
            }
        }

        public async Task<int> GetBookedQuantityByEventIdAsync(Guid eventId)
        {
            try
            {
                return await _context.Tickets
                    .Where(t => t.EventId == eventId)
                    .SumAsync(t => (int?)t.Quantity) ?? 0;
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error retrieving booked quantity for event {eventId}.", ex);
            }
        }

        public async Task<int> GetCountByUserIdAsync(Guid userId)
        {
            try
            {
                return await _context.Tickets
                    .Where(t => t.UserId == userId)
                    .SumAsync(t => t.Quantity);
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error retrieving ticket count for user {userId}.", ex);
            }
        }

        public async Task<IEnumerable<Tickets>> GetByEventIdAsync(Guid eventId)
        {
            try
            {
                return await _context.Tickets
                    .Include(t => t.User)
                    .Where(t => t.EventId == eventId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error retrieving tickets for event {eventId}.", ex);
            }
        }

        public async Task<IEnumerable<(string Type, int Quantity)>> GetTicketTypeCountsByEventIdAsync(Guid eventId)
        {
            try
            {
                return await _context.Tickets
                    .Where(t => t.EventId == eventId)
                    .GroupBy(t => t.Type)
                    .Select(g => new ValueTuple<string, int>(g.Key, g.Sum(t => t.Quantity)))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error retrieving ticket type counts for event {eventId}.", ex);
            }
        }

        public async Task<IEnumerable<Events>> GetDistinctEventsByUserIdAsync(Guid userId)
        {
            try
            {
                return await _context.Tickets
                    .Include(t => t.Event)
                    .Where(t => t.UserId == userId && t.Event != null)
                    .Select(t => t.Event)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error retrieving distinct events for user {userId}.", ex);
            }
        }

        public async Task AddRangeAsync(IEnumerable<Tickets> tickets)
        {
            try
            {
                await _context.Tickets.AddRangeAsync(tickets);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error adding ticket range.", ex);
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error saving ticket changes.", ex);
            }
        }
    }
}
