using Event_Management.Helpers;
using Eventpro.Domain.Interfaces.ITicket;
using Eventpro.Domain.Interfaces.IUser;
using Eventpro.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Event_Management.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ITicketService _ticketService;

        public UserController(
            IUserService userService,
            ITicketService ticketService)
        {
            _userService = userService;
            _ticketService = ticketService;
        }

        [HttpGet]
        [Route("my-tickets")]
        public async Task<IActionResult> UserEvent(string eventName, string status)
        {
            try
            {
                // Extract user ID from token
                Guid userId = TokenHelper.GetIdFromToken(Request);

                // Get user
                var userResponse = await _userService.GetUserByIdAsync(userId);
                if (!userResponse.Success || userResponse.Data == null)
                {
                    return RedirectToAction("Signup", "Account");
                }
                var user = userResponse.Data;

                // Get tickets
                var ticketsResponse = await _ticketService.GetTicketsByUserIdAsync(userId, eventName, status);
                if (!ticketsResponse.Success)
                {
                    return BadRequest(ticketsResponse.Message);
                }
                var tickets = ticketsResponse.Data ?? Enumerable.Empty<Tickets>();

                // Get distinct events
                var eventsResponse = await _ticketService.GetDistinctEventsByUserIdAsync(userId);
                if (!eventsResponse.Success)
                {
                    return BadRequest(eventsResponse.Message);
                }
                var events = eventsResponse.Data ?? Enumerable.Empty<Events>();

                // Fill ViewBag
                ViewBag.User = user;
                ViewBag.Tickets = tickets.ToList();
                ViewBag.Events = events.ToList();
                ViewBag.SearchQuery = eventName;
                ViewBag.StatusFilter = status;
                ViewBag.Role = user.Role;

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return RedirectToAction("Index", "Home");
            }
        }

    }
}
