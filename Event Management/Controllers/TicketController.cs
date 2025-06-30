using Event_Management.Data;
using Event_Management.Helpers;
using Event_Management.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;
using System.Text.Json;

namespace Event_Management.Controllers
{
    public class TicketController : Controller
    {
        private readonly AppDbContext _context;

        public TicketController(AppDbContext context)
        {
            _context = context;
        }

        // Get the events by organisation Id (token)
        [HttpGet]
        [Route("all-events")]
        public async Task<IActionResult> AllEvents(string eventName, string status, string type, string venue)
        {
            Guid userId;

            try
            {
                userId = TokenHelper.GetIdFromToken(Request);
            }
            catch (Exception)
            {
                return RedirectToAction("Signup", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || user.Role != "User")
            {
                return RedirectToAction("Signup", "Account");
            }

            var currentDateTime = DateTime.Now;

            var eventsQuery = _context.Events
                .Where(e => e.Type != "Organizer"
                    && e.Status != "Completed"
                    && e.Status != "Cancelled"
                    && e.DateTime > currentDateTime
                    && (e.RegistrationDeadline == null || e.RegistrationDeadline > currentDateTime));


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

            if (!string.IsNullOrWhiteSpace(type))
            {
                eventsQuery = eventsQuery
                    .Where(e => e.Type == type);
            }

            if (!string.IsNullOrWhiteSpace(venue))
            {
                eventsQuery = eventsQuery
                    .Where(e => e.Venue == venue);
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
        [Route("event/ticket/{id}")]
        public async Task<IActionResult> Event(Guid id)
        {
            Guid userId;

            try
            {
                userId = TokenHelper.GetIdFromToken(Request);
            }
            catch (Exception)
            {
                return RedirectToAction("Signup", "Account");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.Role != "User")
            {
                return RedirectToAction("Signup", "Account");
            }

            var eventData = await _context.Events
                                          .FirstOrDefaultAsync(e => e.Id == id);

            if (eventData == null)
            {
                return NotFound("Event not found");
            }

            var organizer = await _context.Users
                                            .FirstOrDefaultAsync(u => u.Id == eventData.UserId);

            if (organizer == null)
            {
                return NotFound("Event creator not found");
            }

            int bookedQuantity = await _context.Tickets
            .Where(t => t.EventId == id)
            .SumAsync(t => (int?)t.Quantity) ?? 0;

            int? remainingTickets = eventData.MaxAttendees.HasValue
                ? eventData.MaxAttendees - bookedQuantity
                : (int?)null;

            var model = new EventsView
            {
                User = user,
                Event = eventData,
                Organizer = organizer,
                RemainingTickets = remainingTickets,
            };

            return View(model);
        }

        // Get the event by event id for ticket booking
        [HttpGet]
        [Route("event/buy-tickets/{id}")]
        public async Task<IActionResult> BuyTickets(Guid id)
        {
            Guid userId;

            try
            {
                userId = TokenHelper.GetIdFromToken(Request);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Token parsing failed: " + ex.Message);
                return RedirectToAction("Create", "Account");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.Role != "User")
            {
                return RedirectToAction("Signup", "Account");
            }

            var eventData = await _context.Events
                                          .FirstOrDefaultAsync(e => e.Id == id);

            if (eventData == null)
            {
                return NotFound("Event not found");
            }

            var model = new EventsView
            {
                User = user,
                Event = eventData
            };

            return View(model);
        }

        [HttpPost]
        [Route("ticket/book/{id}")]
        public async Task<IActionResult> BookTickets(Guid id, [FromForm] Dictionary<string, int> quantities)
        {
            if (quantities == null || quantities.All(q => q.Value <= 0))
            {
                ModelState.AddModelError("", "Please select at least one ticket.");
                return RedirectToAction("BuyTickets", "Ticket", new { id });
            }

            // Get user
            Guid userId;
            try
            {
                userId = TokenHelper.GetIdFromToken(Request);
            }
            catch
            {
                TempData["ErrorMessage"] = "You must be signed in to book tickets.";
                return RedirectToAction("Signup", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || user.Role != "User")
            {
                TempData["ErrorMessage"] = "Invalid user account.";
                return RedirectToAction("Signup", "Account");
            }

            var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction("AllEvents", "Event");
            }

            int existingTickets = await _context.Tickets
                .Where(t => t.EventId == ev.Id)
                .SumAsync(t => t.Quantity);

            int totalNewQuantity = quantities.Sum(q => q.Value);
            int availableSeats = (ev.MaxAttendees ?? 0) - existingTickets;

            // Free Events
            if (ev.IsPaid != "Paid")
            {
                if (totalNewQuantity <= 0)
                {
                    ModelState.AddModelError("", "Please select at least one ticket.");
                    var model = new EventsView { Event = ev };
                    return View("BuyTickets", model);
                }

                if (ev.MaxAttendees > 0 && existingTickets + totalNewQuantity > ev.MaxAttendees)
                {
                    string msg = availableSeats <= 0
                        ? "This event is sold out."
                        : $"Not enough seats available. Only {availableSeats} ticket(s) remaining.";
                    ModelState.AddModelError("", msg);
                    var model = new EventsView { Event = ev };
                    return View("BuyTickets", model);
                }

                var freeTickets = quantities
                    .Where(q => q.Value > 0)
                    .Select(kvp => new Tickets
                    {
                        EventId = ev.Id,
                        UserId = userId,
                        Quantity = kvp.Value,
                        Type = kvp.Key,
                        TicketPrice = 0,
                        BookingFee = 0,
                        TotalPrice = 0,
                        PaymentStatus = "Confirmed",
                        PurchaseDate = DateTime.Now
                    }).ToList();

                await _context.Tickets.AddRangeAsync(freeTickets);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Successfully registered for {totalNewQuantity} free ticket(s).";
                return RedirectToAction("AllEvents", "Ticket");
            }

            // Paid Events: Parse pricing with helper
            var pricingDict = TicketPricingHelper.ParsePricing(ev.TicketPricing);

            if (!pricingDict.Any())
            {
                ModelState.AddModelError("", "No pricing information found for this event.");
                var model = new EventsView { Event = ev };
                return View("BuyTickets", model);
            }

            if (ev.MaxAttendees > 0 && existingTickets + totalNewQuantity > ev.MaxAttendees)
            {
                string msg = availableSeats <= 0
                    ? "This event is sold out."
                    : $"Not enough seats available. Only {availableSeats} ticket(s) remaining.";
                ModelState.AddModelError("", msg);
                var model = new EventsView { Event = ev };
                return View("BuyTickets", model);
            }

            // Create paid tickets
            var paidTickets = new List<Tickets>();
            foreach (var kvp in quantities.Where(q => q.Value > 0))
            {
                if (!pricingDict.TryGetValue(kvp.Key, out decimal unitPrice))
                {
                    ModelState.AddModelError("", $"Invalid ticket type selected: {kvp.Key}");
                    var model = new EventsView { Event = ev };
                    return View("BuyTickets", model);
                }

                decimal bookingFee = Math.Ceiling(unitPrice * kvp.Value * 0.03m);
                decimal totalPrice = (unitPrice * kvp.Value) + bookingFee;

                paidTickets.Add(new Tickets
                {
                    EventId = ev.Id,
                    UserId = userId,
                    Quantity = kvp.Value,
                    Type = kvp.Key,
                    TicketPrice = unitPrice,
                    BookingFee = bookingFee,
                    TotalPrice = totalPrice,
                    PaymentStatus = "Confirmed",
                    PurchaseDate = DateTime.Now
                });
            }

            await _context.Tickets.AddRangeAsync(paidTickets);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Successfully booked {totalNewQuantity} ticket(s).";
            return RedirectToAction("UserEvent", "User");
        }


    }
}
