using Eventpro.Domain.Exceptions;
using Eventpro.Domain.Interfaces.IEvents;
using Eventpro.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Eventpro.DataAccess
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _context;
        public EventRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Events>> GetAllAsync()
        {
            try
            {
                return await _context.Events
                    .Include(e => e.User)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error retrieving all events.", ex);
            }
        }

        public async Task<IEnumerable<Events>> GetFilteredAsync(
            Guid? eventId = null,
            string? name = null,
            string? organizedBy = null,
            string? type = null,
            string? venue = null,
            string? status = null,
            string? isPaid = null)
        {
            try
            {
                var q = _context.Events
                    .Include(e => e.User)
                    .AsQueryable();

                if (eventId.HasValue)
                    q = q.Where(e => e.Id == eventId.Value);

                if (!string.IsNullOrWhiteSpace(name))
                    q = q.Where(e => EF.Functions.Like(e.Name, $"%{name.Trim()}%"));

                if (!string.IsNullOrWhiteSpace(organizedBy))
                    q = q.Where(e => EF.Functions.Like(e.User.Name, $"%{organizedBy.Trim()}%"));

                if (!string.IsNullOrWhiteSpace(type))
                    q = q.Where(e => e.Type == type.Trim());

                if (!string.IsNullOrWhiteSpace(venue))
                    q = q.Where(e => EF.Functions.Like(e.Venue, $"%{venue.Trim()}%"));

                if (!string.IsNullOrWhiteSpace(status))
                    q = q.Where(e => e.Status == status.Trim());

                if (!string.IsNullOrWhiteSpace(isPaid))
                    q = q.Where(e => e.IsPaid == isPaid.Trim());

                return await q
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error filtering events.", ex);
            }
        }

        public async Task<Events?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Events
                    .Include(e => e.User)
                    .FirstOrDefaultAsync(e => e.Id == id);
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error retrieving event with ID {id}.", ex);
            }
        }

        public async Task<IEnumerable<Events>> GetByUserIdAsync(Guid userId)
        {
            try
            {
                return await _context.Events
                    .Where(e => e.UserId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error retrieving events for user {userId}.", ex);
            }
        }

        public async Task AddAsync(Events evt)
        {
            try
            {
                await _context.Events.AddAsync(evt);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error adding event.", ex);
            }
        }

        public Task UpdateAsync(Events evt)
        {
            try
            {
                _context.Events.Update(evt);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error updating event.", ex);
            }
        }

        public Task DeleteAsync(Events evt)
        {
            try
            {
                _context.Events.Remove(evt);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error deleting event.", ex);
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
                throw new DatabaseException("Error saving changes to events.", ex);
            }
        }
    }
}
