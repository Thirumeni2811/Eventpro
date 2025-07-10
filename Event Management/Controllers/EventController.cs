using Event_Management.Helpers;
using Eventpro.Domain.Interfaces.IEvents;
using Eventpro.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Event_Management.Controllers
{
    [Authorize(Roles = "Organizer")]
    public class EventController : Controller
    {

        private readonly IEventService _eventService;
        private readonly IUserService _userService;

        public EventController(IEventService eventService, IUserService userService)
        {
            _eventService = eventService;
            _userService = userService;
        }

        // Step -1 : Basic Event information
        [HttpGet]
        [Route("/create-event")]
        public async Task<IActionResult> Basics()
        {
            // Extract userId from token
            var userId = TokenHelper.GetIdFromToken(Request);
            if (userId == Guid.Empty)
                return RedirectToAction("Create", "Account");

            // Get user from UserService
            var response = await _userService.GetUserByIdAsync(userId);

            if (response == null || !response.Success || response.Data == null)
                return RedirectToAction("Create", "Account");

            var user = response.Data;

            if (user.Role != "Organizer")
                return RedirectToAction("Create", "Account");

            return View();
        }

        //step - 2 : Venue information
        [HttpGet]
        [Route("/venue-details")]
        public IActionResult Offline()
        {
            var eventJson = HttpContext.Session.GetString("EventData");

            if (string.IsNullOrEmpty(eventJson))
            {
                ModelState.AddModelError(string.Empty, "Session expired or missing. Please restart the process.");
                return RedirectToAction("Basics", "Event"); 
            }

            var model = JsonSerializer.Deserialize<Events>(eventJson);

            if (model?.Venue?.Trim().ToLower() == "Online")
            {
                return RedirectToAction("Tickets", "Event");
            }
            return View();
        }

        //step - 3 : Ticketing and Registration
        [HttpGet]
        [Route("/tickets")]
        public IActionResult Tickets()
        {
            var eventJson = HttpContext.Session.GetString("EventData");

            if (string.IsNullOrEmpty(eventJson))
            {
                ModelState.AddModelError(string.Empty, "Session expired or missing. Please restart the process.");
                return RedirectToAction("Basics", "Event");
            }

            var model = new Events(); 
            return View(model);
        }

        //step - 4 : program and schedule
        [HttpGet]
        [Route("/program-schedule")]
        public IActionResult Schedule()
        {
            var eventJson = HttpContext.Session.GetString("EventData");

            if (string.IsNullOrEmpty(eventJson))
            {
                ModelState.AddModelError(string.Empty, "Session expired or missing. Please restart the process.");
                return RedirectToAction("Basics", "Event");
            }

            return View();
        }

        //step - 5 : promotion and communication
        [HttpGet]
        [Route("/promotion")]
        public IActionResult Promotion()
        {
            var eventJson = HttpContext.Session.GetString("EventData");

            if (string.IsNullOrEmpty(eventJson))
            {
                ModelState.AddModelError(string.Empty, "Session expired or missing. Please restart the process.");
                return RedirectToAction("Basics", "Event");
            }

            return View();
        }

        //step - 6 : Technical and AV Requirements
        [HttpGet]
        [Route("/technical-requirements")]
        public IActionResult Technical()
        {
            var eventJson = HttpContext.Session.GetString("EventData");

            if (string.IsNullOrEmpty(eventJson))
            {
                ModelState.AddModelError(string.Empty, "Session expired or missing. Please restart the process.");
                return RedirectToAction("Basics", "Event");
            }

            return View();
        }

        //step - 7 : Staffs management
        [HttpGet]
        [Route("/staffs-management")]
        public IActionResult Staffs()
        {
            var eventJson = HttpContext.Session.GetString("EventData");

            if (string.IsNullOrEmpty(eventJson))
            {
                ModelState.AddModelError(string.Empty, "Session expired or missing. Please restart the process.");
                return RedirectToAction("Basics", "Event");
            }

            return View();
        }

        //step - 8 : Catering
        [HttpGet]
        [Route("/catering")]
        public IActionResult Catering()
        {
            var eventJson = HttpContext.Session.GetString("EventData");

            if (string.IsNullOrEmpty(eventJson))
            {
                ModelState.AddModelError(string.Empty, "Session expired or missing. Please restart the process.");
                return RedirectToAction("Basics", "Event");
            }

            return View();
        }

        //step - 9 : Post events
        [HttpGet]
        [Route("/post-event")]
        public IActionResult Post()
        {
            var eventJson = HttpContext.Session.GetString("EventData");

            if (string.IsNullOrEmpty(eventJson))
            {
                ModelState.AddModelError(string.Empty, "Session expired or missing. Please restart the process.");
                return RedirectToAction("Basics", "Event");
            }

            return View();
        }

        //step - 10 : Event Info
        [HttpGet]
        [Route("/event-info")]
        public IActionResult Main()
        {
            var eventJson = HttpContext.Session.GetString("EventData");
            if (string.IsNullOrEmpty(eventJson))
            {
                ModelState.AddModelError(string.Empty, "Session expired or missing. Please restart the process.");
                return RedirectToAction("Basics", "Event");
            }

            var model = JsonSerializer.Deserialize<Events>(eventJson);
            return View(model);
        }

        // Step -1 : Basic Event information
        [HttpPost]
        [Route("/create-event")]
        public IActionResult Basics(Events model)
        {
            EventValidationHelper.ValidateBasics(model, ModelState);

            if (!ModelState.IsValid)
            {
                return View(model);
            }


            try
            {
                var eventJson = JsonSerializer.Serialize(model);
                HttpContext.Session.SetString("EventData", eventJson);

                var venue = model.Venue?.Trim().ToLower();
                var type = model.Type?.Trim().ToLower();

                if (type == "organizer" || venue == "offline")
                {
                    return RedirectToAction("Offline", "Event");
                }

                else if (venue == "online" && type != "organizer")
                {
                    return RedirectToAction("Tickets", "Event");
                }

                else
                {
                    return RedirectToAction("Basics", "Event");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                return View(model);
            }
        }

        //step - 2 : Venue information
        [HttpPost]
        [Route("/venue-details")]
        public IActionResult Offline(Events model)
        {
            EventValidationHelper.ValidateVenue(model, ModelState);

            if (!ModelState.IsValid)
            {
                return View(model); 
            }

            try
            {
                var eventJson = HttpContext.Session.GetString("EventData");

                if (string.IsNullOrEmpty(eventJson))
                {
                    Console.WriteLine("No event data found in session.");
                    ModelState.AddModelError(string.Empty, "Session expired or missing. Please restart the process.");
                    return View(model);
                }

                var existingEvent = JsonSerializer.Deserialize<Events>(eventJson);

                if (existingEvent != null)
                {
                    existingEvent.VenueName = model.VenueName;
                    existingEvent.Address = model.Address;
                    existingEvent.Environment = model.Environment;
                    existingEvent.Capacity = model.Capacity;
                    existingEvent.Accessibility = model.Accessibility;
                }

                var updatedJson = JsonSerializer.Serialize(existingEvent);
                HttpContext.Session.SetString("EventData", updatedJson);

                var type = existingEvent?.Type?.Trim().ToLower();
                if (type == "organizer")
                {
                    return RedirectToAction("Schedule", "Event");
                }
                else
                {
                    return RedirectToAction("Tickets", "Event");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                return View(model);
            }
        }


        //step - 3 : Ticketing and Registration
        [HttpPost]
        [Route("/tickets")]
        public IActionResult Tickets(Events model)
        {
            if (string.IsNullOrWhiteSpace(model.IsPaid))
            {
                ModelState.AddModelError("IsPaid", "Payment type (Paid/Free) is required.");
            }

            EventValidationHelper.ValidateTickets(model, ModelState);

            try
            {
                var eventJson = HttpContext.Session.GetString("EventData");

                if (string.IsNullOrEmpty(eventJson))
                {
                    ModelState.AddModelError(string.Empty, "Session expired or missing. Please restart the process.");
                    return View(model);
                }

                var existingEvent = JsonSerializer.Deserialize<Events>(eventJson);

                // Additional validation for RegistrationDeadline vs Event Date
                if (existingEvent != null && model.RegistrationDeadline.HasValue)
                {
                    if (model.RegistrationDeadline.Value >= existingEvent.DateTime)
                    {
                        ModelState.AddModelError("RegistrationDeadline", "Registration deadline must be before the event date.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (existingEvent != null)
                {
                    existingEvent.IsPaid = model.IsPaid;
                    existingEvent.TicketPricing = model.TicketPricing;
                    existingEvent.Payment = model.Payment;
                    existingEvent.MaxAttendees = model.MaxAttendees;
                    existingEvent.RegistrationDeadline = model.RegistrationDeadline;
                    existingEvent.CancellationPolicy = model.CancellationPolicy;

                    var updatedJson = JsonSerializer.Serialize(existingEvent);
                    HttpContext.Session.SetString("EventData", updatedJson);
                }

                return RedirectToAction("Schedule", "Event");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                return View(model);
            }
        }

        //step - 4 : program and schedule
        [HttpPost]
        [Route("/program-schedule")]
        public IActionResult Schedule(Events model)
        {
            EventValidationHelper.ValidateProgram(model, ModelState);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var eventJson = HttpContext.Session.GetString("EventData");

                if (string.IsNullOrEmpty(eventJson))
                {
                    ModelState.AddModelError(string.Empty, "Session expired or missing. Please restart.");
                    return View(model);
                }

                var existingEvent = JsonSerializer.Deserialize<Events>(eventJson);

                if (existingEvent != null)
                {
                    existingEvent.Agenda = model.Agenda;
                    existingEvent.Activities = model.Activities;
                    existingEvent.Speakers = model.Speakers;
                    existingEvent.Breaks = model.Breaks;

                    var updatedJson = JsonSerializer.Serialize(existingEvent);
                    HttpContext.Session.SetString("EventData", updatedJson);
                }

                return RedirectToAction("Promotion", "Event");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                return View(model);
            }
        }


        //step - 5 : promotion and communication
        [HttpPost]
        [Route("/promotion")]
        public async Task<IActionResult> Promotion(
            [FromForm] Events model,
            IFormFile BannerFile
        )
        {
            EventValidationHelper.ValidatePromotions(model, BannerFile, ModelState);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (BannerFile == null || BannerFile.Length == 0)
                {
                    ModelState.AddModelError("BannerFile", "Banner file is required.");
                    return View(model);
                }

                model.Banner = await FileUploadHelper.SaveFileAsync(BannerFile, "banners");

                var eventJson = HttpContext.Session.GetString("EventData");

                if (string.IsNullOrEmpty(eventJson))
                {
                    ModelState.AddModelError("", "Session expired. Please restart the process.");
                    return View(model);
                }

                var existingEvent = JsonSerializer.Deserialize<Events>(eventJson);

                if (existingEvent != null)
                {
                    existingEvent.Banner = model.Banner;
                    existingEvent.Platforms = model.Platforms;
                    existingEvent.Audience = model.Audience;
                    existingEvent.Sponsors = model.Sponsors;
                }

                var updatedJson = JsonSerializer.Serialize(existingEvent);
                HttpContext.Session.SetString("EventData", updatedJson);

                return RedirectToAction("Technical", "Event");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                return View(model);
            }
        }


        //step - 6 : Technical and AV Requirements
        [HttpPost]
        [Route("/technical-requirements")]
        public IActionResult Technical(Events model)
        {
            try
            {
                var eventJson = HttpContext.Session.GetString("EventData");
                if (string.IsNullOrEmpty(eventJson))
                {
                    ModelState.AddModelError("", "Session expired. Please start again.");
                    return View(model);
                }

                var existingEvent = JsonSerializer.Deserialize<Events>(eventJson);
                if (existingEvent != null)
                {
                    existingEvent.SoundSystem = model.SoundSystem;
                    existingEvent.Projection = model.Projection;
                    existingEvent.LiveStreaming = model.LiveStreaming;
                    existingEvent.Internet = model.Internet;
                    existingEvent.PowerBackup = model.PowerBackup;

                    var updatedJson = JsonSerializer.Serialize(existingEvent);
                    HttpContext.Session.SetString("EventData", updatedJson);
                }

                return RedirectToAction("Staffs", "Event");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                return View(model);
            }
        }


        //step - 7 : Staffs management
        [HttpPost]
        [Route("/staffs-management")]
        public IActionResult Staffs(Events model)
        {
            try
            {
                var eventJson = HttpContext.Session.GetString("EventData");
                if (string.IsNullOrEmpty(eventJson))
                {
                    ModelState.AddModelError("", "Session expired. Please start again.");
                    return View(model);
                }

                var existingEvent = JsonSerializer.Deserialize<Events>(eventJson);
                if (existingEvent != null)
                {
                    existingEvent.Volunteers = model.Volunteers;
                    existingEvent.Security = model.Security;
                    existingEvent.Coordinators = model.Coordinators;
                    existingEvent.Medical = model.Medical;

                    var updatedJson = JsonSerializer.Serialize(existingEvent);
                    HttpContext.Session.SetString("EventData", updatedJson);
                }

                var venue = existingEvent?.Venue?.Trim().ToLower();
                if (venue == "Online")
                {
                    return RedirectToAction("Post", "Event");
                }
                else
                {
                    return RedirectToAction("Catering", "Event");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                return View(model);
            }
        }


        //step - 8 : Catering
        [HttpPost]
        [Route("/catering")]
        public IActionResult Catering(Events model)
        {
            EventValidationHelper.ValidateCatering(model, ModelState);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var eventJson = HttpContext.Session.GetString("EventData");
                if (string.IsNullOrEmpty(eventJson))
                {
                    ModelState.AddModelError("", "Session expired. Please start again.");
                    return View(model);
                }

                var existingEvent = JsonSerializer.Deserialize<Events>(eventJson);

                if (existingEvent != null)
                {
                    existingEvent.Veg = model.Veg;
                    existingEvent.NonVeg = model.NonVeg;
                    existingEvent.Menu = model.Menu;
                    existingEvent.ServingStyle = model.ServingStyle;
                    existingEvent.GuestCount = model.GuestCount;

                    var updatedJson = JsonSerializer.Serialize(existingEvent);
                    HttpContext.Session.SetString("EventData", updatedJson);
                }

                return RedirectToAction("Post", "Event");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                return View(model);
            }
        }

        //step - 9 : Post events
        [HttpPost]
        [Route("/post-event")]
        public IActionResult Post(Events model)
        {
            try
            {
                var eventJson = HttpContext.Session.GetString("EventData");

                if (string.IsNullOrEmpty(eventJson))
                {
                    ModelState.AddModelError(string.Empty, "Session expired or missing. Please restart the process.");
                    return View(model);
                }

                var existingEvent = JsonSerializer.Deserialize<Events>(eventJson);

                if (existingEvent != null)
                {
                    existingEvent.Feedback = model.Feedback;
                    existingEvent.Media = model.Media;
                    existingEvent.Report = model.Report;
                    existingEvent.Thanks = model.Thanks;

                    var updatedJson = JsonSerializer.Serialize(existingEvent);
                    HttpContext.Session.SetString("EventData", updatedJson);
                }

                return RedirectToAction("Main", "Event");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                return View(model);
            }
        }


        //step - 10 : Event Info
        [HttpPost]
        [Route("/event-info")]
        public async Task<IActionResult> Main(Events model, IFormFile? BannerFile)
        {
            Guid userId;
            try
            {
                userId = TokenHelper.GetIdFromToken(Request);
                if (userId == Guid.Empty)
                    throw new Exception();
            }
            catch
            {
                return RedirectToAction("Create", "Account");
            }

            var json = HttpContext.Session.GetString("EventData");
            var draft = !string.IsNullOrEmpty(json)
                ? JsonSerializer.Deserialize<Events>(json)
                : null;
            if (draft != null)
                model.Banner = draft.Banner;

            model.UserId = userId;
            model.Status = "Upcoming";
            model.CreatedAt = DateTime.UtcNow;

            EventValidationHelper.ValidateBasics(model, ModelState);
            EventValidationHelper.ValidateVenue(model, ModelState);
            EventValidationHelper.ValidateTickets(model, ModelState);
            EventValidationHelper.ValidateProgram(model, ModelState);
            EventValidationHelper.ValidateCatering(model, ModelState);

            if (BannerFile != null && BannerFile.Length > 0)
            {
                try
                {
                    model.Banner = await FileUploadHelper.SaveFileAsync(BannerFile, "banners");
                }
                catch
                {
                    ModelState.AddModelError("BannerFile", "Invalid banner file.");
                }
            }
            else if (draft != null && !string.IsNullOrEmpty(draft.Banner))
            {
                model.Banner = draft.Banner;
            }

            EventValidationHelper.ValidatePromotions(model, BannerFile, ModelState);

            ModelState.Remove("User");

            if (!ModelState.IsValid)
            {
                foreach (var kvp in ModelState)
                    foreach (var err in kvp.Value.Errors)
                        Console.WriteLine($"Validation error on '{kvp.Key}': {err.ErrorMessage}");

                return View(model);
            }

            try
            {
                var result = await _eventService.CreateEventAsync(
                    model,
                    actingRole: "Organizer",
                    actingUserId: userId
                );

                if (!result.Success)
                {
                    ModelState.AddModelError("", result.Message);
                    return View(model);
                }

                return RedirectToAction("OrgEvents", "Organizer");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving event: {ex}");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }
    }
}
