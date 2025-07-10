using Event_Management.Helpers;
using Event_Management.ViewModels;
using Eventpro.Domain.Interfaces.IEvents;
using Eventpro.Domain.Interfaces.ITicket;
using Eventpro.Domain.Interfaces.IUser;
using Eventpro.Domain.Models;
using Eventpro.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Event_Management.Controllers
{
    [Authorize(Roles = "Organizer")]
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
                Guid userId = TokenHelper.GetIdFromToken(Request);

                var userResponse = await _userService.GetUserByIdAsync(userId);
                if (!userResponse.Success || userResponse.Data == null || userResponse.Data.Role != "Organizer")
                {
                    return RedirectToAction("Create", "Account");
                }

                // Validate user input
                EventValidationHelper.ValidateBasics(model, ModelState);
                EventValidationHelper.ValidateVenue(model, ModelState);
                EventValidationHelper.ValidateTickets(model, ModelState);
                EventValidationHelper.ValidateProgram(model, ModelState);
                EventValidationHelper.ValidateCatering(model, ModelState);

                ModelState.Remove("User");

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Fetch the existing event
                var existingResponse = await _eventService.GetEventByIdAsync(id);
                if (!existingResponse.Success || existingResponse.Data == null)
                {
                    TempData["ErrorMessage"] = "Event not found.";
                    return RedirectToAction("OrgEvents");
                }

                var existingEvent = existingResponse.Data;

                // Update only the relevant fields
                existingEvent.Name = model.Name;
                existingEvent.Type = model.Type;
                existingEvent.Description = model.Description;
                existingEvent.Theme = model.Theme;
                existingEvent.DateTime = model.DateTime;
                existingEvent.Duration = model.Duration;
                existingEvent.Venue = model.Venue;
                existingEvent.VenueName = model.VenueName;
                existingEvent.Address = model.Address;
                existingEvent.Environment = model.Environment;
                existingEvent.Capacity = model.Capacity;
                existingEvent.Accessibility = model.Accessibility;
                existingEvent.IsPaid = model.IsPaid;
                existingEvent.TicketPricing = model.TicketPricing;
                existingEvent.Payment = model.Payment;
                existingEvent.MaxAttendees = model.MaxAttendees;
                existingEvent.RegistrationDeadline = model.RegistrationDeadline;
                existingEvent.CancellationPolicy = model.CancellationPolicy;
                existingEvent.Agenda = model.Agenda;
                existingEvent.Activities = model.Activities;
                existingEvent.Speakers = model.Speakers;
                existingEvent.Breaks = model.Breaks;
                existingEvent.Platforms = model.Platforms;
                existingEvent.Audience = model.Audience;
                existingEvent.Sponsors = model.Sponsors;
                existingEvent.SoundSystem = model.SoundSystem;
                existingEvent.Projection = model.Projection;
                existingEvent.LiveStreaming = model.LiveStreaming;
                existingEvent.Internet = model.Internet;
                existingEvent.PowerBackup = model.PowerBackup;
                existingEvent.Volunteers = model.Volunteers;
                existingEvent.Security = model.Security;
                existingEvent.Coordinators = model.Coordinators;
                existingEvent.Medical = model.Medical;
                existingEvent.Veg = model.Veg;
                existingEvent.NonVeg = model.NonVeg;
                existingEvent.Menu = model.Menu;
                existingEvent.ServingStyle = model.ServingStyle;
                existingEvent.GuestCount = model.GuestCount;
                existingEvent.Feedback = model.Feedback;
                existingEvent.Media = model.Media;
                existingEvent.Report = model.Report;
                existingEvent.Thanks = model.Thanks;
                existingEvent.Message = model.Message;

                // Process the banner
                if (BannerFile != null && BannerFile.Length > 0)
                {
                    try
                    {
                        existingEvent.Banner = await FileUploadHelper.SaveFileAsync(BannerFile, "banners");
                    }
                    catch
                    {
                        ModelState.AddModelError("BannerFile", "Invalid banner file.");
                        return View(existingEvent);
                    }
                }

                EventValidationHelper.ValidatePromotions(existingEvent, BannerFile, ModelState);

                if (!ModelState.IsValid)
                {
                    return View(existingEvent);
                }

                existingEvent.UserId = userId;

                // Call update
                var updateResponse = await _eventService.UpdateEventAsync(existingEvent, "Organizer", userId);

                if (!updateResponse.Success)
                {
                    ModelState.AddModelError("", updateResponse.Message);
                    return View(existingEvent);
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
