using Event_Management.Helpers;
using Event_Management.ViewModels;
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
                Guid userId = TokenHelper.GetIdFromToken(Request);

                var userResponse = await _userService.GetUserByIdAsync(userId);
                if (!userResponse.Success || userResponse.Data == null)
                {
                    return RedirectToAction("Signup", "Account");
                }
                var user = userResponse.Data;

                var ticketsResponse = await _ticketService.GetTicketsByUserIdAsync(userId, eventName, status);
                if (!ticketsResponse.Success)
                {
                    return BadRequest(ticketsResponse.Message);
                }
                var tickets = ticketsResponse.Data ?? Enumerable.Empty<Tickets>();

                var eventsResponse = await _ticketService.GetDistinctEventsByUserIdAsync(userId);
                if (!eventsResponse.Success)
                {
                    return BadRequest(eventsResponse.Message);
                }

                var model = new UserTicketsViewModel
                {
                    User = user,
                    Tickets = ticketsResponse.Data?.ToList() ?? new List<Tickets>(),
                    Events = eventsResponse.Data?.ToList() ?? new List<Events>(),
                    Role = user.Role
                };

                // Keep search filters in ViewData
                ViewData["SearchQuery"] = eventName;
                ViewData["StatusFilter"] = status;

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return RedirectToAction("Index", "Home");
            }
        }

    }
}
