using Event_Management.Data;
using Event_Management.Helpers;
using Event_Management.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Event_Management.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // Get the events by User Id (token)
        [HttpGet]
        [Route("my-tickets")]
        public async Task<IActionResult> UserEvent(string eventName, string status)
        {
            Guid userId;

            try
            {
                userId = TokenHelper.GetIdFromToken(Request);
            }
            catch
            {
                return RedirectToAction("Signup", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return RedirectToAction("Signup", "Account");
            }

            var ticketsQuery = _context.Tickets
                .Include(t => t.Event)
                    .ThenInclude(e => e.User)
                .Include(t => t.User)
                .Where(t => t.UserId == userId);

            if (!string.IsNullOrWhiteSpace(eventName))
            {
                eventName = eventName.Trim();
                ticketsQuery = ticketsQuery.Where(t => t.Event.Name.Contains(eventName));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                status = status.Trim();
                ticketsQuery = ticketsQuery.Where(t => t.Event.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            var tickets = await ticketsQuery
                .OrderByDescending(t => t.PurchaseDate)
                .ToListAsync();

            Console.WriteLine($"Tickets fetched: {tickets.Count}");

            var viewModel = new EventsView
            {
                User = user,
                Tickets = tickets,
                Events = tickets.Select(t => t.Event).Distinct().ToList(),
                SearchQuery = eventName,
                StatusFilter = status,
                Role = user.Role
            };

            return View(viewModel);
        }

    }
}
