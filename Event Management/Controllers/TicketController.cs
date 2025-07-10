using Event_Management.Data;
using Event_Management.Helpers;
using Event_Management.ViewModels;
using Eventpro.Domain.Exceptions;
using Eventpro.Domain.Interfaces.IEvents;
using Eventpro.Domain.Interfaces.ITicket;
using Eventpro.Domain.Interfaces.IUser;
using Eventpro.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Event_Management.Controllers
{
    [Authorize(Roles = "User")]
    public class TicketController : Controller
    {
        private readonly IUserService _userService;
        private readonly ITicketService _ticketService;
        private readonly IEventService _eventService;

        public TicketController(IUserService userService, IEventService eventService, ITicketService ticketService)
        {
            _userService = userService;
            _ticketService = ticketService;
            _eventService = eventService;
        }

        // Get all events
        [HttpGet]
        [Route("all-events")]
        public async Task<IActionResult> AllEvents(string eventName, string status, string type, string venue)
        {
            try
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

                var userResponse = await _userService.GetUserByIdAsync(userId);

                if (userResponse == null || !userResponse.Success || userResponse.Data == null)
                {
                    return RedirectToAction("Signup", "Account");
                }

                var user = userResponse.Data;

                if (user.Role != "User")
                {
                    return RedirectToAction("Signup", "Account");
                }

                var eventsResponse = await _eventService.GetPublicEventsAsync(
                    name: eventName,
                    status: status,
                    type: type,
                    venue: venue);

                if (eventsResponse == null)
                {
                    throw new Exception("Events response is null.");
                }

                if (!eventsResponse.Success)
                {
                    return BadRequest(eventsResponse.Message);
                }

                var events = eventsResponse.Data ?? Enumerable.Empty<Events>();

                var model = new AllEventsViewModel
                {
                    User = user,
                    Events = events.ToList(),
                    Role = user.Role
                };

                // Keep search/filter in ViewBag as requested
                ViewBag.SearchQuery = eventName;
                ViewBag.StatusFilter = status;
                ViewBag.TypeFilter = type;
                ViewBag.VenueFilter = venue;

                return View(model);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return RedirectToAction("Index", "Home");
            }
        }

        // Get the event by event id
        [HttpGet]
        [Route("event/ticket/{id}")]
        public async Task<IActionResult> Event(Guid id)
        {
            try
            {
                Guid userId = TokenHelper.GetIdFromToken(Request);

                var userResponse = await _userService.GetUserByIdAsync(userId);
                if (!userResponse.Success || userResponse.Data == null || userResponse.Data.Role != "User")
                {
                    return RedirectToAction("Signup", "Account");
                }
                var user = userResponse.Data;

                var eventResponse = await _eventService.GetEventByIdAsync(id);
                if (!eventResponse.Success || eventResponse.Data == null)
                {
                    return NotFound("Event not found");
                }
                var eventData = eventResponse.Data;

                var organizerResponse = await _userService.GetUserByIdAsync(eventData.UserId);
                if (!organizerResponse.Success || organizerResponse.Data == null)
                {
                    return NotFound("Event creator not found");
                }
                var organizer = organizerResponse.Data;

                var bookedQuantityResponse = await _ticketService.GetBookedQuantityAsync(id);
                if (!bookedQuantityResponse.Success)
                {
                    return BadRequest(bookedQuantityResponse.Message);
                }
                int bookedQuantity = bookedQuantityResponse.Data;

                int? remainingTickets = eventData.MaxAttendees.HasValue
                    ? eventData.MaxAttendees - bookedQuantity
                    : (int?)null;

                var model = new EventTicketViewModel
                {
                    User = user,
                    Event = eventData,
                    Organizer = organizer,
                    RemainingTickets = remainingTickets
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return RedirectToAction("Index", "Home");
            }
        }

        // Get the event by event id for ticket booking
        [HttpGet]
        [Route("event/buy-tickets/{id}")]
        public async Task<IActionResult> BuyTickets(Guid id)
        {
            try
            {
                Guid userId = TokenHelper.GetIdFromToken(Request);

                var userResponse = await _userService.GetUserByIdAsync(userId);
                if (!userResponse.Success || userResponse.Data == null || userResponse.Data.Role != "User")
                {
                    return RedirectToAction("Signup", "Account");
                }
                var user = userResponse.Data;

                var eventResponse = await _eventService.GetEventByIdAsync(id);
                if (!eventResponse.Success || eventResponse.Data == null)
                {
                    return NotFound("Event not found");
                }
                var eventData = eventResponse.Data;

                var model = new BuyTicketsViewModel
                {
                    User = user,
                    Event = eventData
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return RedirectToAction("AllEvents", "Ticket");
            }
        }


        [HttpPost]
        [Route("ticket/book/{id}")]
        public async Task<IActionResult> BookTickets(Guid id, [FromForm] Dictionary<string, int> quantities)
        {
            try
            {
                if (quantities == null || quantities.All(q => q.Value <= 0))
                {
                    ModelState.AddModelError("", "Please select at least one ticket.");
                    return RedirectToAction("BuyTickets", "Ticket", new { id });
                }

                // Get userId
                Guid userId = TokenHelper.GetIdFromToken(Request);

                // Get user
                var userResponse = await _userService.GetUserByIdAsync(userId);
                if (!userResponse.Success || userResponse.Data == null || userResponse.Data.Role != "User")
                {
                    TempData["ErrorMessage"] = "Invalid user account.";
                    return RedirectToAction("Signup", "Account");
                }
                var user = userResponse.Data;

                // Get event
                var eventResponse = await _eventService.GetEventByIdAsync(id);
                if (!eventResponse.Success || eventResponse.Data == null)
                {
                    TempData["ErrorMessage"] = "Event not found.";
                    return RedirectToAction("AllEvents", "Event");
                }
                var ev = eventResponse.Data;

                // Get booked quantity
                var bookedQuantityResponse = await _ticketService.GetBookedQuantityAsync(ev.Id);
                if (!bookedQuantityResponse.Success)
                {
                    TempData["ErrorMessage"] = bookedQuantityResponse.Message;
                    return RedirectToAction("AllEvents", "Event");
                }
                int existingTickets = bookedQuantityResponse.Data;

                int totalNewQuantity = quantities.Sum(q => q.Value);
                int availableSeats = (ev.MaxAttendees ?? 0) - existingTickets;

                // Free Events
                if (ev.IsPaid != "Paid")
                {
                    if (totalNewQuantity <= 0)
                    {
                        ModelState.AddModelError("", "Please select at least one ticket.");
                        ViewBag.Event = ev;
                        return View("BuyTickets");
                    }

                    if (ev.MaxAttendees > 0 && existingTickets + totalNewQuantity > ev.MaxAttendees)
                    {
                        string msg = availableSeats <= 0
                            ? "This event is sold out."
                            : $"Not enough seats available. Only {availableSeats} ticket(s) remaining.";
                        ModelState.AddModelError("", msg);
                        ViewBag.Event = ev;
                        return View("BuyTickets");
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

                    await _ticketService.AddTicketsAsync(freeTickets);

                    TempData["SuccessMessage"] = $"Successfully registered for {totalNewQuantity} free ticket(s).";
                    return RedirectToAction("AllEvents", "Ticket");
                }

                // Paid Events
                var pricingDict = TicketPricingHelper.ParsePricing(ev.TicketPricing);
                if (!pricingDict.Any())
                {
                    ModelState.AddModelError("", "No pricing information found for this event.");
                    ViewBag.Event = ev;
                    return View("BuyTickets");
                }

                if (ev.MaxAttendees > 0 && existingTickets + totalNewQuantity > ev.MaxAttendees)
                {
                    string msg = availableSeats <= 0
                        ? "This event is sold out."
                        : $"Not enough seats available. Only {availableSeats} ticket(s) remaining.";
                    ModelState.AddModelError("", msg);
                    ViewBag.Event = ev;
                    return View("BuyTickets");
                }

                var paidTickets = new List<Tickets>();
                foreach (var kvp in quantities.Where(q => q.Value > 0))
                {
                    if (!pricingDict.TryGetValue(kvp.Key, out decimal unitPrice))
                    {
                        ModelState.AddModelError("", $"Invalid ticket type selected: {kvp.Key}");
                        ViewBag.Event = ev;
                        return View("BuyTickets");
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

                await _ticketService.AddTicketsAsync(paidTickets);

                TempData["SuccessMessage"] = $"Successfully booked {totalNewQuantity} ticket(s).";
                return RedirectToAction("UserEvent", "User");
            }
            catch (ServiceException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("AllEvents", "Event");
            }
        }

    }
}
