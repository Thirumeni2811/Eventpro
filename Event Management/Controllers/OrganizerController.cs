using Event_Management.Data;
using Event_Management.Helpers;
using Event_Management.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Event_Management.Controllers
{
    public class OrganizerController : Controller
    {
        private readonly AppDbContext _context;

        public OrganizerController(AppDbContext context)
        {
            _context = context;
        }

        // Get the events by organisation Id (token)
        [HttpGet]
        [Route("organisation")]
        public async Task<IActionResult> OrgEvents(string eventName, string status)
        {
            Guid userId;

            try
            {
                userId = TokenHelper.GetIdFromToken(Request);
            }
            catch (Exception)
            {
                return RedirectToAction("Create", "Account");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(o => o.Id == userId);

            if (user == null || user.Role != "Organizer")
            {
                return RedirectToAction("Create", "Account");
            }

            var eventsQuery = _context.Events
                .Where(e => e.UserId == userId);

            if (!string.IsNullOrWhiteSpace(eventName))
            {
                eventName = eventName.Trim();
                eventsQuery = eventsQuery
                    .Where(e => e.Name.Contains(eventName));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                eventsQuery = eventsQuery
                    .Where(e => e.Status == status);
            }

            var events = await eventsQuery.ToListAsync();

            bool isUpdated = false;
            foreach (var ev in events)
            {
                if (ev.Status == "Upcoming" && ev.DateTime < DateTime.Now)
                {
                    ev.Status = "Completed";
                    isUpdated = true;
                }
            }

            if (isUpdated)
            {
                await _context.SaveChangesAsync();
            }

            var viewModel = new EventsView
            {
                User = user,
                Events = events,
                SearchQuery = eventName,
                StatusFilter = status,
                Role = user.Role
            };

            return View(viewModel);
        }

        // Get the event by event id
        [HttpGet]
        [Route("event/{id}")]
        public async Task<IActionResult> Event(Guid id)
        {
            Guid userId;

            try
            {
                userId = TokenHelper.GetIdFromToken(Request);
            }
            catch (Exception)
            {
                return RedirectToAction("Create", "Account");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.Role != "Organizer")
            {
                return NotFound("User not found.");
            }

            var eventData = await _context.Events
                                          .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (eventData == null)
            {
                return NotFound("Event not found");
            }

            var ticketsCount = await _context.Tickets
                .Where(t => t.EventId == id)
                .SumAsync(t => (int?)t.Quantity) ?? 0;

            var ticketsByType = await _context.Tickets
                .Where(t => t.EventId == id)
                .GroupBy(t => t.Type)
                .Select(g => new TicketTypeCount
                {
                    Type = g.Key,
                    Quantity = g.Sum(t => t.Quantity)
                })
                .ToListAsync();

            var model = new EventsView
            {
                User = user,
                Event = eventData,
                Role = user.Role,
                TicketsCount = ticketsCount,
                TicketTypeCounts = ticketsByType
            };

            return View(model);
        }

        // Get the event by event id for edit
        [HttpGet]
        [Route("update-event/{id}")]
        public async Task<IActionResult> Update(Guid id)
        {
            Guid userId;
            try
            {
                userId = TokenHelper.GetIdFromToken(Request);
            }
            catch
            {
                return RedirectToAction("Create", "Account");
            }
   
            var user = await _context.Users.FindAsync(userId);

            if (user == null || user.Role != "Organizer")
            {
                return RedirectToAction("Create", "Account");
            }

            var eventData = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (eventData == null)
            {
                return NotFound("Event not found");
            }

            return View(eventData);
        }


        // Update the event by event id
        [HttpPost]
        [Route("update-event/{id}")]
        public async Task<IActionResult> Update(Guid id, Eventsss model)
        {
            Guid userId;
            try
            {
                userId = TokenHelper.GetIdFromToken(Request);
            }
            catch
            {
                return RedirectToAction("Create", "Account");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.Role != "Organizer")
            {
                return RedirectToAction("Create", "Account");
            }

            var eventData = await _context.Events
                                          .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
            if (eventData == null)
            {
                return NotFound("Event not found or unauthorized.");
            }

            // Validations
            EventValidationHelper.ValidateBasics(model, ModelState);
            EventValidationHelper.ValidateVenue(model, ModelState);
            EventValidationHelper.ValidateTickets(model, ModelState);
            EventValidationHelper.ValidateProgram(model, ModelState);
            EventValidationHelper.ValidateCatering(model, ModelState);

            // File upload
            if (model.BannerFile != null && model.BannerFile.Length > 0)
            {
                try
                {
                    var bannerPath = await FileUploadHelper.SaveFileAsync(model.BannerFile, "banners");
                    model.Banner = bannerPath;
                }
                catch
                {
                    ModelState.AddModelError("BannerFile", "Invalid banner file.");
                }
            }
            else
            {
                model.Banner = eventData.Banner;
            }


            EventValidationHelper.ValidatePromotions(model, ModelState);

            if (!ModelState.IsValid)
            {
                // Log each error to the console
                foreach (var kvp in ModelState)
                {
                    var key = kvp.Key;
                    foreach (var error in kvp.Value.Errors)
                    {
                        Console.WriteLine($"Validation error on '{key}': {error.ErrorMessage}");
                    }
                }

                return View(model);
            }


            try
            {
                // Basic Info
                eventData.Name = model.Name;
                eventData.Type = model.Type;
                eventData.Description = model.Description;
                eventData.Theme = model.Theme;
                eventData.DateTime = model.DateTime;
                eventData.Duration = model.Duration;
                eventData.Venue = model.Venue;

                // Venue
                eventData.VenueName = model.VenueName;
                eventData.Address = model.Address;
                eventData.Environment = model.Environment;
                eventData.Capacity = model.Capacity;
                eventData.Accessibility = model.Accessibility;

                // Tickets
                eventData.IsPaid = model.IsPaid;
                eventData.TicketPricing = model.TicketPricing;
                eventData.Payment = model.Payment;
                eventData.MaxAttendees = model.MaxAttendees;
                eventData.RegistrationDeadline = model.RegistrationDeadline;
                eventData.CancellationPolicy = model.CancellationPolicy;

                // Schedule
                eventData.Agenda = model.Agenda;
                eventData.Activities = model.Activities;
                eventData.Speakers = model.Speakers;
                eventData.Breaks = model.Breaks;

                // Promotions
                if (!string.IsNullOrEmpty(model.Banner))
                    eventData.Banner = model.Banner;

                eventData.Platforms = model.Platforms;
                eventData.Audience = model.Audience;
                eventData.Sponsors = model.Sponsors;

                // Technical
                eventData.SoundSystem = model.SoundSystem;
                eventData.Projection = model.Projection;
                eventData.LiveStreaming = model.LiveStreaming;
                eventData.Internet = model.Internet;
                eventData.PowerBackup = model.PowerBackup;

                // Staff
                eventData.Volunteers = model.Volunteers;
                eventData.Security = model.Security;
                eventData.Coordinators = model.Coordinators;
                eventData.Medical = model.Medical;

                // Catering
                eventData.Veg = model.Veg;
                eventData.NonVeg = model.NonVeg;
                eventData.Menu = model.Menu;
                eventData.ServingStyle = model.ServingStyle;
                eventData.GuestCount = model.GuestCount;

                // Post Event
                eventData.Feedback = model.Feedback;
                eventData.Media = model.Media;
                eventData.Report = model.Report;
                eventData.Thanks = model.Thanks;

                //eventData.Status = model.Status;
                eventData.Message = model.Message;

                // Save
                _context.Events.Update(eventData);
                await _context.SaveChangesAsync();

                return RedirectToAction("OrgEvents", "Organizer");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while saving to the database.");
                return View(model);
            }
        }

    }
}
