using Event_Management.Data;
using Event_Management.Helpers;
using Eventpro.Domain.Interfaces.IGallery;
using Eventpro.Domain.Interfaces.IServ;
using Eventpro.Domain.Interfaces.IUser;
using Eventpro.Domain.Models;
using Eventpro.Domain.ResponseFormat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Event_Management.Controllers
{
    public class AdminController : Controller
    {

        private readonly IServService _servService;
        private readonly IGalleryService _galleryService;
        private readonly IUserRepository _userRepository;

        public AdminController(IServService servService, IGalleryService galleryService, IUserRepository userRepository)
        {
            _servService = servService;
            _galleryService = galleryService;
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


        // GET: Provide
        [HttpGet]
        [Route("admin/provide")]
        public async Task<IActionResult> Provide(string title, Guid? id)
        {
            var user = await GetAdminUser();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var provideQuery = _context.Provides.AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                title = title.Trim();
                provideQuery = provideQuery.Where(s => s.Title.Contains(title));
            }

            var provideList = await provideQuery.ToListAsync();

            Provide selectedProvide = null;
            if (id.HasValue)
            {
                selectedProvide = await _context.Provides.FirstOrDefaultAsync(s => s.Id == id.Value);
            }

            ViewBag.ProvideList = provideList;
            ViewData["TitleQuery"] = title;

            ModelState.Clear();

            return View(selectedProvide ?? new Provide());
        }

        // CREATE AND UPDATE - Service
        [HttpPost("admin/provide")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Provide(Provide model)
        {
            var user = await GetAdminUser();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrWhiteSpace(model.Title))
                ModelState.AddModelError("Title", "Title is required.");

            if (string.IsNullOrWhiteSpace(model.Description))
                ModelState.AddModelError("Description", "Description is required.");

            if (!ModelState.IsValid)
            {
                // Reload the list to show in the view again
                ViewBag.ProvideList = await _context.Provides.ToListAsync();
                return View(model);
            }

            if (model.Id != Guid.Empty)
            {
                // UPDATE
                var existing = await _context.Provides.FindAsync(model.Id);
                if (existing == null)
                    return NotFound();

                existing.Title = model.Title;
                existing.Description = model.Description;

                if (model.BannerFile != null && model.BannerFile.Length > 0)
                {
                    var imagePath = await FileUploadHelper.SaveFileAsync(model.BannerFile, "provide");
                    existing.Img = imagePath;
                }
            }
            else
            {
                // CREATE
                string imagePath = "";
                if (model.BannerFile != null && model.BannerFile.Length > 0)
                {
                    imagePath = await FileUploadHelper.SaveFileAsync(model.BannerFile, "provide");
                }
                else
                {
                    ModelState.AddModelError("BannerFile", "Please upload an image.");
                    ViewBag.ProvideList = await _context.Provides.ToListAsync();
                    return View(model);
                }

                var newProvide = new Provide
                {
                    Id = Guid.NewGuid(),
                    Title = model.Title,
                    Description = model.Description,
                    Img = imagePath
                };

                _context.Provides.Add(newProvide);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Provide");
        }

        // DELETE - Provide
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("admin/provide/delete/{id}")]
        public async Task<IActionResult> DeleteProvide(Guid id)
        {
            var user = await GetAdminUser();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var provide = await _context.Provides.FindAsync(id);
            if (provide == null)
                return NotFound();

            _context.Provides.Remove(provide);
            await _context.SaveChangesAsync();

            return RedirectToAction("Provide");
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

        // GET: Users
        [HttpGet("admin/users")]
        public async Task<IActionResult> Users(string userId, string name, string email, string phoneNo, string role, Guid? id)
        {
            var user = await GetAdminUser();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(userId))
            {
                userId = userId.Trim();
                query = query.Where(u => u.Id.ToString().Contains(userId));
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                query = query.Where(u => u.Name.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                email = email.Trim();
                query = query.Where(u => u.Email.Contains(email));
            }

            if (!string.IsNullOrWhiteSpace(phoneNo))
            {
                phoneNo = phoneNo.Trim();
                query = query.Where(u => u.PhoneNo.Contains(phoneNo));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                role = role.Trim();
                query = query.Where(u => u.Role == role);
            }

            var usersList = await query.ToListAsync();

            Users selectedUser = null;
            if (id.HasValue)
            {
                selectedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id.Value);
            }

            ViewBag.UsersList = usersList;

            ViewData["Name"] = name;
            ViewData["Email"] = email;
            ViewData["Phone"] = phoneNo;
            ViewData["Role"] = role;

            ModelState.Clear();

            return View(selectedUser ?? new Users());
        }

        // CREATE AND UPDATE - Users
        [HttpPost("admin/users")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Users(Users model)
        {
            var user = await GetAdminUser();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

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
                ViewBag.UsersList = await _context.Users.ToListAsync();
                return View(model);
            }

            if (model.Id != Guid.Empty)
            {
                // UPDATE
                var existing = await _context.Users.FindAsync(model.Id);
                if (existing == null)
                    return NotFound();

                existing.Name = model.Name;
                existing.Email = model.Email;
                existing.PhoneNo = model.PhoneNo;
                existing.Password = existing.Password;
                existing.Role = model.Role;
            }
            else
            {
                // CREATE
                var newUser = new Users
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    Email = model.Email,
                    PhoneNo = model.PhoneNo,
                    Password = PasswordHelper.HashPassword(model.Password),
                    Role = model.Role
                };

                _context.Users.Add(newUser);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Users");
        }

        // DELETE - Users
        [HttpPost("admin/users/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var use= await GetAdminUser();
            if (use == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Users");
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

        // ADMIN

        // Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin()
        {
            var user = new Users
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                Email = "admin@gmail.com",
                PhoneNo = "9090909090",
                Role = "Admin",
                Password = PasswordHelper.HashPassword("admin@123"),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Admin user created successfully.";
            return RedirectToAction("CreateAdminUser");
        }

        // Get - Login
        [HttpGet]
        [Route("admin-login")]
        public IActionResult Login()
        {
            return View();
        }

        // Post - Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("admin-login")]
        public async Task<IActionResult> Login(Login model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || !PasswordHelper.VerifyPassword(model.Password, user.Password))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            if (user.Role != "Admin")
            {
                return RedirectToAction("Index", "Home");
            }

            //string token = _jwt.GenerateToken(user.Id.ToString(), user.Email);
            string token = "wreiuo346tuyhj";

            HttpContext.Session.SetString("Token", token);

            return RedirectToAction("Service", "Admin");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

    }
}
