using Event_Management.Helpers;
using Eventpro.Domain.Interfaces.IEvents;
using Eventpro.Domain.Interfaces.ITicket;
using Eventpro.Domain.Interfaces.IUser;
using Eventpro.Domain.Models;
using Eventpro.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Event_Management.Controllers
{
    public class OrganizerController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEventService _eventService;
        private readonly ITicketService _ticketService;

        public OrganizerController(IEventService eventService, IUserService userService, ITicketService ticketService)
        {
            _userService = userService;
            _eventService = eventService;
            _ticketService = ticketService;
        }

        // Get the events by organisation Id (token)
        [HttpGet]
        [Route("organisation")]
        public async Task<IActionResult> OrgEvents(string eventName, string status)
        {
            try
            {
                Guid userId = TokenHelper.GetIdFromToken(Request);

                var userResponse = await _userService.GetUserByIdAsync(userId);

                if (!userResponse.Success || userResponse.Data == null || userResponse.Data.Role != "Organizer")
                {
                    return RedirectToAction("Create", "Account");
                }

                var eventsResponse = await _eventService.GetEventsByUserIdAsync(
                    userId,
                    eventName,
                    status);

                if (!eventsResponse.Success)
                {
                    TempData["ErrorMessage"] = eventsResponse.Message;
                    return RedirectToAction("Create", "Account");
                }

                ViewBag.User = userResponse.Data;
                ViewBag.Events = eventsResponse.Data ?? Enumerable.Empty<Events>();
                ViewBag.SearchQuery = eventName;
                ViewBag.StatusFilter = status;
                ViewBag.Role = userResponse.Data.Role;

                return View();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return RedirectToAction("Create", "Account");
            }
        }

        // Get the event by event id
        [HttpGet]
        [Route("event/{id}")]
        public async Task<IActionResult> Event(Guid id)
        {
            try
            {
                Guid userId = TokenHelper.GetIdFromToken(Request);

                var userResponse = await _userService.GetUserByIdAsync(userId);
                if (!userResponse.Success || userResponse.Data == null || userResponse.Data.Role != "Organizer")
                {
                    return NotFound("User not found or not authorized.");
                }

                var user = userResponse.Data;

                var eventResponse = await _eventService.GetEventByIdAsync(id);
                if (!eventResponse.Success || eventResponse.Data == null || eventResponse.Data.UserId != userId)
                {
                    return NotFound("Event not found or not owned by you.");
                }

                var eventData = eventResponse.Data;

                var ticketsResponse = await _ticketService.GetTicketsByEventIdAsync(id, "Organizer");
                if (!ticketsResponse.Success)
                {
                    TempData["ErrorMessage"] = ticketsResponse.Message;
                    return RedirectToAction("OrgEvents");
                }

                var tickets = ticketsResponse.Data?.ToList() ?? new List<Tickets>();

                int ticketsCount = tickets.Sum(t => t.Quantity);

                var ticketCountsResponse = await _ticketService.GetTicketTypeCountsByEventIdAsync(id);
                if (!ticketCountsResponse.Success)
                {
                    TempData["ErrorMessage"] = ticketCountsResponse.Message;
                    return RedirectToAction("OrgEvents");
                }

                var ticketTypeCounts = ticketCountsResponse.Data ?? Enumerable.Empty<(string Type, int Quantity)>();

                ViewBag.User = user;
                ViewBag.Event = eventData;
                ViewBag.Role = user.Role;
                ViewBag.TicketsCount = ticketsCount;
                ViewBag.TicketTypeCounts = ticketTypeCounts;

                return View();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return RedirectToAction("OrgEvents");
            }
        }

        // Get the event by event id for edit
        [HttpGet]
        [Route("update-event/{id}")]
        public async Task<IActionResult> Update(Guid id)
        {
            try
            {
                Guid userId = TokenHelper.GetIdFromToken(Request);

                var userResponse = await _userService.GetUserByIdAsync(userId);
                if (!userResponse.Success || userResponse.Data == null || userResponse.Data.Role != "Organizer")
                {
                    return RedirectToAction("Create", "Account");
                }

                var eventResponse = await _eventService.GetEventByIdAsync(id);
                if (!eventResponse.Success || eventResponse.Data == null || eventResponse.Data.UserId != userId)
                {
                    TempData["ErrorMessage"] = "Event not found or not authorized.";
                    return RedirectToAction("OrgEvents");
                }

                ViewBag.Event = eventResponse.Data;

                return View();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return RedirectToAction("OrgEvents");
            }
        }

        // Update the event by event id
        [HttpPost]
        [Route("update-event/{id}")]
        public async Task<IActionResult> Update(Guid id, Events model, IFormFile BannerFile)
        {
            try
            {
                Guid userId = TokenHelper.GetIdFromToken(Request);

                var userResponse = await _userService.GetUserByIdAsync(userId);
                if (!userResponse.Success || userResponse.Data == null || userResponse.Data.Role != "Organizer")
                {
                    return RedirectToAction("Create", "Account");
                }

                EventValidationHelper.ValidateBasics(model, ModelState);
                EventValidationHelper.ValidateVenue(model, ModelState);
                EventValidationHelper.ValidateTickets(model, ModelState);
                EventValidationHelper.ValidateProgram(model, ModelState);
                EventValidationHelper.ValidateCatering(model, ModelState);

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var existingResponse = await _eventService.GetEventByIdAsync(id);
                if (!existingResponse.Success || existingResponse.Data == null)
                {
                    TempData["ErrorMessage"] = "Event not found.";
                    return RedirectToAction("OrgEvents");
                }

                if (BannerFile != null && BannerFile.Length > 0)
                {
                    try
                    {
                        model.Banner = await FileUploadHelper.SaveFileAsync(BannerFile, "banners");
                    }
                    catch
                    {
                        ModelState.AddModelError("BannerFile", "Invalid banner file.");
                        return View(model);
                    }
                }
                else
                {
                    model.Banner = existingResponse.Data.Banner;
                }

                EventValidationHelper.ValidatePromotions(model, BannerFile, ModelState);

                model.Id = id;
                model.UserId = userId;

                var updateResponse = await _eventService.UpdateEventAsync(
                    model, "Organizer", userId);

                if (!updateResponse.Success)
                {
                    ModelState.AddModelError("", updateResponse.Message);
                    return View(model);
                }

                return RedirectToAction("OrgEvents", "Organizer");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update error: {ex.Message}");
                ModelState.AddModelError("", "An unexpected error occurred.");
                return View(model);
            }
        }

    }
}
