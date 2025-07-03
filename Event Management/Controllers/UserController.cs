using Event_Management.Helpers;
using Event_Management.Models;
using Eventpro.Domain.Interfaces.ITicket;
using Eventpro.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Event_Management.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ITicketService _ticketService;
        private readonly ITicketRepository _ticketRepository;

        public UserController(
            IUserService userService,
            ITicketService ticketService,
            ITicketRepository ticketRepository)
        {
            _userService = userService;
            _ticketService = ticketService;
            _ticketRepository = ticketRepository;
        }

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

            // Get the user via service
            var userResponse = await _userService.GetUserByIdAsync(userId);
            if (!userResponse.Success || userResponse.Data == null)
            {
                return RedirectToAction("Signup", "Account");
            }

            var user = userResponse.Data;

            // Get the tickets via service
            var ticketsResponse = await _ticketService.GetTicketsByUserIdAsync(userId, eventName, status);
            if (!ticketsResponse.Success)
            {
                return BadRequest(ticketsResponse.Message);
            }

            var tickets = ticketsResponse.Data ?? Enumerable.Empty<Tickets>();

            // Get distinct events via service
            var eventsResponse = await _ticketService.GetDistinctEventsByUserIdAsync(userId);
            if (!eventsResponse.Success)
            {
                return BadRequest(eventsResponse.Message);
            }

            var events = eventsResponse.Data ?? Enumerable.Empty<Events>();

            // Pass everything to ViewBag
            ViewBag.User = user;
            ViewBag.Tickets = tickets.ToList();
            ViewBag.Events = events.ToList();
            ViewBag.SearchQuery = eventName;
            ViewBag.StatusFilter = status;
            ViewBag.Role = user.Role;

            return View();
        }
    }
}
