using Event_Management.Data;
using Event_Management.Helpers;
using Event_Management.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Event_Management.Controllers
{
    public class EventController : Controller
    {

        private readonly AppDbContext _context;

        public EventController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsTokenValid()
        {
            return HttpContext.Request.Cookies.TryGetValue("token", out string? token) && !string.IsNullOrWhiteSpace(token);
        }

        // Step -1 : Basic Event information
        [HttpGet]
        [Route("/create-event")]
        public async Task<IActionResult> Basics()
        {
            if (!IsTokenValid())
                return RedirectToAction("Create", "Account");

            // Extract userId from token
            var userId = TokenHelper.GetIdFromToken(Request);
            if (userId == Guid.Empty)
                return RedirectToAction("Create", "Account");

            // Get user from Users table
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            // Check if user exists and is Organizer
            if (user == null || user.Role != "Organizer")
                return RedirectToAction("Create", "Account");

            return View();
        }


        //step - 2 : Venue information
        [HttpGet]
        [Route("/venue-details")]
        public IActionResult Offline()
        {
            if (!IsTokenValid())
                return RedirectToAction("Create", "Account");

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
            if (!IsTokenValid())
                return RedirectToAction("Create", "Account");

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
            if (!IsTokenValid())
                return RedirectToAction("Create", "Account");

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
            if (!IsTokenValid())
                return RedirectToAction("Create", "Account");

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
            if (!IsTokenValid())
                return RedirectToAction("Create", "Account");

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
            if (!IsTokenValid())
                return RedirectToAction("Create", "Account");

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
            if (!IsTokenValid())
                return RedirectToAction("Create", "Account");

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
            if (!IsTokenValid())
                return RedirectToAction("Create", "Account");

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
            if (!IsTokenValid())
                return RedirectToAction("Create", "Account");

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
        public async Task<IActionResult> Promotion([FromForm] Events model)
        {
            EventValidationHelper.ValidatePromotions(model, ModelState);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (model.BannerFile == null || model.BannerFile.Length == 0)
                {
                    ModelState.AddModelError("BannerFile", "Banner file is required.");
                    return View(model);
                }

                model.Banner = await FileUploadHelper.SaveFileAsync(model.BannerFile, "banners");

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
        public async Task<IActionResult> Main(Events model)
        {
            var userId = TokenHelper.GetIdFromToken(Request);
            if (userId == Guid.Empty)
                return RedirectToAction("Create", "Account");

            var eventJson = HttpContext.Session.GetString("EventData");
            var sessionEvent = !string.IsNullOrEmpty(eventJson) ? JsonSerializer.Deserialize<Events>(eventJson) : null;

            if (sessionEvent != null)
            {
                model.Banner = sessionEvent.Banner; 
            }

            model.UserId = userId;
            model.Status = "Upcoming";
            model.CreatedAt = DateTime.Now;

            // Validation
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
            else if (sessionEvent != null && !string.IsNullOrEmpty(sessionEvent.Banner))
            {
                model.Banner = sessionEvent.Banner;
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
                await _context.Events.AddAsync(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("OrgEvents", "Organizer");
            }
            catch
            {
                ModelState.AddModelError("", "An error occurred while saving to the database.");
                return View(model);
            }
        }
    }
}
