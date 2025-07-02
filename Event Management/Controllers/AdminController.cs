using Event_Management.Data;
using Event_Management.Helpers;
using Eventpro.Domain.Interfaces.IGallery;
using Eventpro.Domain.Interfaces.IProvide;
using Eventpro.Domain.Interfaces.IServ;
using Eventpro.Domain.Interfaces.IUser;
using Eventpro.Domain.Models;
using Eventpro.Domain.ResponseFormat;
using Eventpro.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Event_Management.Controllers
{
    public class AdminController : Controller
    {

        private readonly IServService _servService;
        private readonly IProvideService _provideService;
        private readonly IGalleryService _galleryService;
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;

        public AdminController(IServService servService, IProvideService provideService, IGalleryService galleryService, IUserService userService, IUserRepository userRepository)
        {
            _servService = servService;
            _provideService = provideService;
            _galleryService = galleryService;
            _userService = userService;
            _userRepository = userRepository;
        }

        // FUNCTION FOR ADMIN ACCESS
        private async Task<Users> GetAdminUser()
        {
            Guid userId;

            try
            {
                userId = SessionTokenHelper.GetIdFromSession(HttpContext.Session);
            }
            catch
            {
                return null;
            }

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null || user.Role != "Admin")
            {
                return null;
            }

            return user;
        }


        /*--------------------------------------
                   S E R V I C E
        --------------------------------------*/

        // GET
        [HttpGet]
        [Route("admin/services")]
        public async Task<IActionResult> Service(string title, Guid? id)
        {
            try
            {
                var user = await GetAdminUser();
                if (user == null)
                    return RedirectToAction("Index", "Home");

                var listResponse = await _servService.GetAllAsync(title);
                ViewBag.ServicesList = (listResponse.Data ?? Enumerable.Empty<Services>()).ToList();
                ViewData["TitleQuery"] = title;

                Services selectedService = null;
                if (id.HasValue)
                {
                    var getByIdResponse = await _servService.GetByIdAsync(id.Value);
                    selectedService = getByIdResponse.Data;
                }

                ModelState.Clear();
                return View(selectedService ?? new Services());
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while loading services.";
                return RedirectToAction("Index", "Home");
            }
        }

        // CREATE AND UPDATE - Service
        [HttpPost("admin/services")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Service(Services model, IFormFile BannerFile)
        {
            try
            {
                var user = await GetAdminUser();
                if (user == null)
                    return RedirectToAction("Index", "Home");

                if (string.IsNullOrWhiteSpace(model.Title))
                    ModelState.AddModelError("Title", "Title is required.");
                if (string.IsNullOrWhiteSpace(model.Description))
                    ModelState.AddModelError("Description", "Description is required.");

                if (model.Id == Guid.Empty && (BannerFile == null || BannerFile.Length == 0))
                    ModelState.AddModelError("BannerFile", "Please upload an image.");

                if (!ModelState.IsValid)
                {
                    var listResponse = await _servService.GetAllAsync();
                    ViewBag.ServicesList = (listResponse.Data ?? Enumerable.Empty<Services>()).ToList();
                    return View(model);
                }

                if (BannerFile != null && BannerFile.Length > 0)
                {
                    model.Img = await FileUploadHelper.SaveFileAsync(BannerFile, "service");
                }

                IServiceResponse<Services> result;
                if (model.Id == Guid.Empty)
                {
                    result = await _servService.CreateAsync(model, user.Role);
                }
                else
                {
                    result = await _servService.UpdateAsync(model, user.Role);
                }

                if (!result.Success)
                {
                    ModelState.AddModelError("", result.Message);
                    var listResponse = await _servService.GetAllAsync();
                    ViewBag.ServicesList = (listResponse.Data ?? Enumerable.Empty<Services>()).ToList();
                    return View(model);
                }

                return RedirectToAction("Service");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while saving the service.";
                return RedirectToAction("Service");
            }
        }

        // DELETE - Service
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("admin/services/delete/{id}")]
        public async Task<IActionResult> DeleteService(Guid id)
        {
            try
            {
                var user = await GetAdminUser();
                if (user == null)
                    return RedirectToAction("Index", "Home");

                var result = await _servService.DeleteAsync(id);
                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                }

                return RedirectToAction("Service");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting the service.";
                return RedirectToAction("Service");
            }
        }

        /*--------------------------------------
                   P R O V I D E
        --------------------------------------*/

        // GER
        [HttpGet("admin/provide")]
        public async Task<IActionResult> Provide(string title, Guid? id)
        {
            try
            {
                var user = await GetAdminUser();
                if (user == null)
                    return RedirectToAction("Index", "Home");

                var listResponse = await _provideService.GetAllAsync(title);
                ViewBag.ProvideList = (listResponse.Data ?? Enumerable.Empty<Provides>()).ToList();
                ViewData["TitleQuery"] = title;

                Provides selectedProvide = null;
                if (id.HasValue)
                {
                    var getByIdResponse = await _provideService.GetByIdAsync(id.Value);
                    selectedProvide = getByIdResponse.Data;
                }

                ModelState.Clear();
                return View(selectedProvide ?? new Provides());
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while loading provides.";
                return RedirectToAction("Index", "Home");
            }
        }

        // CREATE AND UPDATE
        [HttpPost("admin/provide")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Provide(Provides model, IFormFile BannerFile)
        {
            try
            {
                var user = await GetAdminUser();
                if (user == null)
                    return RedirectToAction("Index", "Home");

                if (string.IsNullOrWhiteSpace(model.Title))
                    ModelState.AddModelError("Title", "Title is required.");

                if (string.IsNullOrWhiteSpace(model.Description))
                    ModelState.AddModelError("Description", "Description is required.");

                if (model.Id == Guid.Empty && (BannerFile == null || BannerFile.Length == 0))
                    ModelState.AddModelError("BannerFile", "Please upload an image.");

                if (!ModelState.IsValid)
                {
                    var listResponse = await _provideService.GetAllAsync();
                    ViewBag.ProvideList = (listResponse.Data ?? Enumerable.Empty<Provides>()).ToList();
                    return View(model);
                }

                if (BannerFile != null && BannerFile.Length > 0)
                {
                    model.Img = await FileUploadHelper.SaveFileAsync(BannerFile, "provide");
                }

                IServiceResponse<Provides> result;
                if (model.Id == Guid.Empty)
                {
                    result = await _provideService.CreateAsync(model, user.Role);
                }
                else
                {
                    result = await _provideService.UpdateAsync(model, user.Role);
                }

                if (!result.Success)
                {
                    ModelState.AddModelError("", result.Message);
                    var listResponse = await _provideService.GetAllAsync();
                    ViewBag.ProvideList = (listResponse.Data ?? Enumerable.Empty<Provides>()).ToList();
                    return View(model);
                }

                return RedirectToAction("Provide");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while saving the provide.";
                return RedirectToAction("Provide");
            }
        }

        // DELETE
        [HttpPost("admin/provide/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProvide(Guid id)
        {
            try
            {
                var user = await GetAdminUser();
                if (user == null)
                    return RedirectToAction("Index", "Home");

                var result = await _provideService.DeleteAsync(id);
                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                }

                return RedirectToAction("Provide");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting the provide.";
                return RedirectToAction("Provide");
            }
        }


        /*--------------------------------------
                   G A L L E R Y
        --------------------------------------*/
        // GET
        [HttpGet("admin/gallery")]
        public async Task<IActionResult> Gallery(string name, string type, Guid? id)
        {
            try
            {
                var user = await GetAdminUser();
                if (user == null)
                    return RedirectToAction("Index", "Home");

                var result = await _galleryService.GetAllAsync(name, type);
                ViewBag.GalleryList = (result.Data ?? Enumerable.Empty<Gallery>()).ToList();

                Gallery selectedGallery = null;
                if (id.HasValue)
                {
                    var getByIdResult = await _galleryService.GetByIdAsync(id.Value);
                    selectedGallery = getByIdResult.Data;
                }

                ViewData["NameQuery"] = name;
                ViewData["TypeFilter"] = type;
                ModelState.Clear();

                return View(selectedGallery ?? new Gallery());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while loading the gallery.";
                return RedirectToAction("Index", "Home");
            }
        }

        // CREATE AND UPDATE - Gallery
        [HttpPost("admin/gallery")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Gallery(Gallery model, IFormFile BannerFile)
        {
            try
            {
                var user = await GetAdminUser();
                if (user == null)
                    return RedirectToAction("Index", "Home");

                if (string.IsNullOrWhiteSpace(model.Name))
                    ModelState.AddModelError("Name", "Name is required.");
                if (string.IsNullOrWhiteSpace(model.Description))
                    ModelState.AddModelError("Description", "Description is required.");
                if (string.IsNullOrWhiteSpace(model.Type))
                    ModelState.AddModelError("Type", "Type is required.");

                if (!ModelState.IsValid)
                {
                    var listResult = await _galleryService.GetAllAsync();
                    ViewBag.GalleryList = (listResult.Data ?? Enumerable.Empty<Gallery>()).ToList();
                    return View(model);
                }

                if (model.Id != Guid.Empty)
                {
                    // UPDATE
                    if (BannerFile != null && BannerFile.Length > 0)
                    {
                        model.Banner = await FileUploadHelper.SaveFileAsync(BannerFile, "gallery");
                    }

                    var updateResult = await _galleryService.UpdateAsync(model);
                    if (!updateResult.Success)
                    {
                        ModelState.AddModelError("", updateResult.Message);
                        var listResult = await _galleryService.GetAllAsync();
                        ViewBag.GalleryList = (listResult.Data ?? Enumerable.Empty<Gallery>()).ToList();
                        return View(model);
                    }
                }
                else
                {
                    // CREATE
                    if (BannerFile == null || BannerFile.Length == 0)
                    {
                        ModelState.AddModelError("BannerFile", "Please upload an image.");
                        var listResult = await _galleryService.GetAllAsync();
                        ViewBag.GalleryList = (listResult.Data ?? Enumerable.Empty<Gallery>()).ToList();
                        return View(model);
                    }

                    model.Banner = await FileUploadHelper.SaveFileAsync(BannerFile, "gallery");

                    var createResult = await _galleryService.CreateAsync(model);
                    if (!createResult.Success)
                    {
                        ModelState.AddModelError("", createResult.Message);
                        var listResult = await _galleryService.GetAllAsync();
                        ViewBag.GalleryList = (listResult.Data ?? Enumerable.Empty<Gallery>()).ToList();
                        return View(model);
                    }
                }

                return RedirectToAction("Gallery");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while saving the gallery.";
                return RedirectToAction("Gallery");
            }
        }

        // DELETE
        [HttpPost("admin/gallery/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGallery(Guid id)
        {
            try
            {
                var user = await GetAdminUser();
                if (user == null)
                    return RedirectToAction("Index", "Home");

                var result = await _galleryService.DeleteAsync(id);
                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                }

                return RedirectToAction("Gallery");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting the gallery.";
                return RedirectToAction("Gallery");
            }
        }

        /*--------------------------------------
                       U S E R
        --------------------------------------*/

        // GET
        [HttpGet("admin/users")]
        public async Task<IActionResult> Users(string userId, string name, string email, string phoneNo, string role, Guid? id)
        {
            try
            {
                var admin = await GetAdminUser();
                if (admin == null)
                    return RedirectToAction("Index", "Home");

                var allUsersResponse = await _userService.GetAllUsersAsync(
                    admin.Role,
                    userId,
                    name,
                    email,
                    phoneNo,
                    role
                );

                if (!allUsersResponse.Success)
                {
                    TempData["ErrorMessage"] = allUsersResponse.Message;
                    return RedirectToAction("Index", "Home");
                }

                var usersList = allUsersResponse.Data.ToList();

                Users selectedUser = null;
                if (id.HasValue)
                {
                    var getUserResponse = await _userService.GetUserByIdAsync(id.Value);
                    if (getUserResponse.Success)
                        selectedUser = getUserResponse.Data;
                }

                ViewBag.UsersList = usersList;
                ViewData["Name"] = name;
                ViewData["Email"] = email;
                ViewData["Phone"] = phoneNo;
                ViewData["Role"] = role;

                ModelState.Clear();

                return View(selectedUser ?? new Users());
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while loading users.";
                return RedirectToAction("Index", "Home");
            }
        }

        // CREATE and UPDATE - Users
        [HttpPost("admin/users")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Users(Users model)
        {
            try
            {
                var admin = await GetAdminUser();
                if (admin == null)
                    return RedirectToAction("Index", "Home");

                if (model.Id == Guid.Empty)
                {
                    if (string.IsNullOrWhiteSpace(model.Password))
                        ModelState.AddModelError(nameof(model.Password), "Password is required.");
                    if (model.Password != model.CPassword)
                        ModelState.AddModelError(nameof(model.CPassword), "Passwords do not match.");
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(model.Password))
                    {
                        if (model.Password != model.CPassword)
                            ModelState.AddModelError(nameof(model.CPassword), "Passwords do not match.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    var allUsersResponse = await _userService.GetAllUsersAsync(admin.Role);
                    ViewBag.UsersList = (allUsersResponse.Data ?? Enumerable.Empty<Users>()).ToList();
                    return View(model);
                }

                IServiceResponse<Users> response;

                if (model.Id == Guid.Empty)
                {
                    // CREATE
                    var createResponse = await _userService.CreateUserProfileAsync(model);
                    if (!createResponse.Success)
                    {
                        ModelState.AddModelError("", createResponse.Message);
                        var allUsersResponse = await _userService.GetAllUsersAsync(admin.Role);
                        ViewBag.UsersList = (allUsersResponse.Data ?? Enumerable.Empty<Users>()).ToList();
                        return View(model);
                    }
                }
                else
                {
                    // UPDATE
                    response = await _userService.UpdateProfileAsync(model.Id, model, admin.Role, admin.Id);
                    if (!response.Success)
                    {
                        ModelState.AddModelError("", response.Message);
                        var allUsersResponse = await _userService.GetAllUsersAsync(admin.Role);
                        ViewBag.UsersList = (allUsersResponse.Data ?? Enumerable.Empty<Users>()).ToList();
                        return View(model);
                    }
                }

                return RedirectToAction("Users");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while saving user.";
                return RedirectToAction("Users");
            }
        }

        // DELETE - Users
        [HttpPost("admin/users/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var admin = await GetAdminUser();
                if (admin == null)
                    return RedirectToAction("Index", "Home");

                var deleteResponse = await _userService.DeleteUserAsync(id, admin.Role, admin.Id);
                if (!deleteResponse.Success)
                {
                    TempData["ErrorMessage"] = deleteResponse.Message;
                }

                return RedirectToAction("Users");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting user.";
                return RedirectToAction("Users");
            }
        }


        // GET: Events
        [HttpGet("/admin/events")]
        public async Task<IActionResult> Events( string eventId, string name, string organizedBy, string type, string venue, string status, string isPaid)
        {
            var user = await GetAdminUser();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var query = _context.Events
                .Include(e => e.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(eventId) && Guid.TryParse(eventId.Trim(), out var eId))
            {
                query = query.Where(e => e.Id == eId);
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                query = query.Where(e => e.Name.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(organizedBy))
            {
                organizedBy = organizedBy.Trim();
                query = query.Where(e => e.User.Name.Contains(organizedBy));
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                type = type.Trim();
                query = query.Where(e => e.Type == type);
            }

            if (!string.IsNullOrWhiteSpace(venue))
            {
                venue = venue.Trim();
                query = query.Where(e => e.Venue.Contains(venue));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                status = status.Trim();
                query = query.Where(e => e.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(isPaid))
            {
                isPaid = isPaid.Trim();
                query = query.Where(e => e.IsPaid == isPaid);
            }

            var eventList = await query
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            var organizers = await _context.Users
                .Where(u => u.Role == "Organizer")
                .OrderBy(u => u.Name)
                .ToListAsync();

            var count = await query.CountAsync();
            ViewBag.EventsCount = count;

            ViewBag.EventsList = await query.ToListAsync();
            ViewBag.Organizers = organizers;
            ViewBag.EventList = eventList;

            ViewData["EventId"] = eventId;
            ViewData["Name"] = name;
            ViewData["OrganizedBy"] = organizedBy;
            ViewData["Type"] = type;
            ViewData["Venue"] = venue;
            ViewData["Status"] = status;
            ViewData["IsPaid"] = isPaid;

            return View();
        }


        // GET: Tickets
        [HttpGet("admin/tickets")]
        public async Task<IActionResult> Ticket( string ticketId, string eventId, string eventName, string organizerName, string buyerName)
        {
            var user = await GetAdminUser();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var query = _context.Tickets
                .Include(t => t.Event)
                .Include(t => t.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(ticketId) && Guid.TryParse(ticketId.Trim(), out var tId))
            {
                query = query.Where(t => t.Id == tId);
            }

            if (!string.IsNullOrWhiteSpace(eventId) && Guid.TryParse(eventId.Trim(), out var eId))
            {
                query = query.Where(t => t.EventId == eId);
            }

            if (!string.IsNullOrWhiteSpace(eventName))
            {
                eventName = eventName.Trim();
                query = query.Where(t => t.Event.Name.Contains(eventName));
            }

            if (!string.IsNullOrWhiteSpace(organizerName))
            {
                organizerName = organizerName.Trim();
                query = query.Where(t => t.Event.User.Name.Contains(organizerName));
            }

            if (!string.IsNullOrWhiteSpace(buyerName))
            {
                buyerName = buyerName.Trim();
                query = query.Where(t => t.User.Name.Contains(buyerName));
            }


            var ticketList = await query
                .OrderByDescending(t => t.PurchaseDate)
                .ToListAsync();

            // Load dropdown data
            var organizers = await _context.Users
                .Where(u => u.Role == "Organizer")
                .OrderBy(u => u.Name)
                .ToListAsync();

            var buyers = await _context.Users
                .Where(u => u.Role == "User")
                .OrderBy(u => u.Name)
                .ToListAsync();

            var events = await _context.Events
                .OrderBy(e => e.Name)
                .ToListAsync();

            ViewBag.Organizers = organizers;
            ViewBag.Buyers = buyers;
            ViewBag.Events = events;
            ViewBag.TicketList = ticketList;

            ViewData["TicketId"] = ticketId;
            ViewData["EventId"] = eventId;
            ViewData["EventName"] = eventName;
            ViewData["OrganizerName"] = organizerName;
            ViewData["BuyerName"] = buyerName;

            return View();
        }

        // GET - Event Details
        [HttpGet]
        [Route("admin/event-details/{id}")]
        public async Task<IActionResult> EventDetails(Guid id)
        {
            var user = await GetAdminUser();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Get Event
            var evt = await _context.Events
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evt == null)
            {
                return NotFound();
            }
            
            var tickets = await _context.Tickets
                .Include(t => t.User)
                .Where(t => t.EventId == id)
                .ToListAsync();

            var ticketTypeCounts = tickets
                .GroupBy(t => t.Type)
                .Select(g => new TicketTypeCount
                {
                    Type = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .ToList();

            var model = new EventsView
            {
                Event = evt,
                Tickets = tickets,
                TicketsCount = tickets.Sum(t => t.Quantity),
                TicketTypeCounts = ticketTypeCounts,
                Organizer = evt.User,
                RemainingTickets = evt.MaxAttendees.HasValue
                    ? evt.MaxAttendees - tickets.Sum(t => t.Quantity)
                    : (int?)null,
                TotalTicketPrice = tickets.Sum(t => t.TicketPrice * t.Quantity),
                TotalBookingFee = tickets.Sum(t => t.BookingFee),
                GrandTotalPrice = tickets.Sum(t => t.TotalPrice)
            };

            return View(model);
        }

        /*--------------------------------------
                       A D M I N
        --------------------------------------*/

        // GET
        [HttpGet]
        [Route("admin-login")]
        public IActionResult Login()
        {
            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("admin-login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                ModelState.AddModelError(nameof(email), "Email is required.");

            if (string.IsNullOrWhiteSpace(password))
                ModelState.AddModelError(nameof(password), "Password is required.");

            if (!ModelState.IsValid)
                return View();

            var result = await _userService.LoginAdminAsync(email, password);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View();
            }

            // Save token in session
            HttpContext.Session.SetString("Token", result.Data.Token);

            return RedirectToAction("Service", "Admin");
        }

        /*--------------------------------------
                     L O G O U T
        --------------------------------------*/
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

    }
}
