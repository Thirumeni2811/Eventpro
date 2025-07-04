using Event_Management.Helpers;
using Event_Management.ViewModels;
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

                var model = new OrganizerEventsViewModel
                {
                    User = userResponse.Data,
                    Events = eventsResponse.Data?.ToList() ?? new List<Events>(),
                    Role = userResponse.Data.Role
                };

                ViewData["SearchQuery"] = eventName;
                ViewData["StatusFilter"] = status;

                return View(model);
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

                var ticketTypeCounts = ticketCountsResponse.Data?
                    .Select(t => new TicketTypeCount
                    {
                        Type = t.Type,
                        Quantity = t.Quantity
                    })
                    .ToList()
                    ?? new List<TicketTypeCount>();

                var model = new OrganizerEventDetailViewModel
                {
                    User = user,
                    Event = eventData,
                    Role = user.Role,
                    Tickets = tickets,
                    TicketsCount = ticketsCount,
                    TicketTypeCounts = ticketTypeCounts
                };

                return View(model);
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

                return View(eventResponse.Data);
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
        public async Task<IActionResult> Update(Guid id, Events model, IFormFile? BannerFile)
        {
            try
            {
                Console.WriteLine("Update called with ID: " + id);

                Guid userId = TokenHelper.GetIdFromToken(Request);
                Console.WriteLine("User ID from token: " + userId);

                var userResponse = await _userService.GetUserByIdAsync(userId);
                Console.WriteLine("User fetched: " + (userResponse.Success ? "Success" : "Fail"));

                if (!userResponse.Success || userResponse.Data == null || userResponse.Data.Role != "Organizer")
                {
                    Console.WriteLine("Unauthorized user or invalid role.");
                    return RedirectToAction("Create", "Account");
                }

                // Validate
                EventValidationHelper.ValidateBasics(model, ModelState);
                EventValidationHelper.ValidateVenue(model, ModelState);
                EventValidationHelper.ValidateTickets(model, ModelState);
                EventValidationHelper.ValidateProgram(model, ModelState);
                EventValidationHelper.ValidateCatering(model, ModelState);

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState invalid. Returning to view.");
                    return View(model);
                }

                var existingResponse = await _eventService.GetEventByIdAsync(id);
                Console.WriteLine("Existing event fetch: " + (existingResponse.Success ? "Found" : "Not Found"));

                if (!existingResponse.Success || existingResponse.Data == null)
                {
                    Console.WriteLine("Event not found.");
                    TempData["ErrorMessage"] = "Event not found.";
                    return RedirectToAction("OrgEvents");
                }

                var existingEvent = existingResponse.Data;

                Console.WriteLine("Updating fields...");

                // Banner handling
                if (BannerFile != null && BannerFile.Length > 0)
                {
                    Console.WriteLine("Banner file detected.");
                    try
                    {
                        existingEvent.Banner = await FileUploadHelper.SaveFileAsync(BannerFile, "banners");
                        Console.WriteLine("Banner uploaded successfully.");
                    }
                    catch
                    {
                        Console.WriteLine("Banner upload failed.");
                        ModelState.AddModelError("BannerFile", "Invalid banner file.");
                        return View(existingEvent);
                    }
                }
                else
                {
                    Console.WriteLine("No new banner provided. Keeping existing.");
                }

                EventValidationHelper.ValidatePromotions(existingEvent, BannerFile, ModelState);

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState invalid after promotion validation.");
                    return View(existingEvent);
                }

                existingEvent.UserId = userId;

                Console.WriteLine("Calling UpdateEventAsync...");
                var updateResponse = await _eventService.UpdateEventAsync(existingEvent, "Organizer", userId);

                if (!updateResponse.Success)
                {
                    Console.WriteLine("Update failed: " + updateResponse.Message);
                    ModelState.AddModelError("", updateResponse.Message);
                    return View(existingEvent);
                }

                Console.WriteLine("Update successful. Redirecting to OrgEvents.");
                return RedirectToAction("OrgEvents", "Organizer");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Update error: " + ex.Message);
                ModelState.AddModelError("", "An unexpected error occurred.");
                return View(model);
            }
        }

    }
}
