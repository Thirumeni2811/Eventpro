using Event_Management.Data;
using Event_Management.Helpers;
using Event_Management.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Event_Management.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly JwtHelper _jwt;

        public AdminController(AppDbContext context, JwtHelper jwt)
        {
            _context = context;
            _jwt = jwt;
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

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Role != "Admin")
            {
                return null;
            }

            return user;
        }


        // GET: Service
        [HttpGet]
        [Route("admin/services")]
        public async Task<IActionResult> Service(string title, Guid? id)
        {
            var user = await GetAdminUser();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var servicesQuery = _context.Services.AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                title = title.Trim();
                servicesQuery = servicesQuery.Where(s => s.Title.Contains(title));
            }

            var servicesList = await servicesQuery.ToListAsync();

            Service selectedService = null;
            if (id.HasValue)
            {
                selectedService = await _context.Services.FirstOrDefaultAsync(s => s.Id == id.Value);
            }

            ViewBag.ServicesList = servicesList;
            ViewData["TitleQuery"] = title;

            ModelState.Clear();

            return View(selectedService ?? new Service());
        }

        // CREATE AND UPDATE - Service
        [HttpPost("admin/services")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Service(Service model)
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
                ViewBag.ServicesList = await _context.Services.ToListAsync();
                return View(model);
            }

            if (model.Id != Guid.Empty)
            {
                var existing = await _context.Services.FindAsync(model.Id);
                if (existing == null)
                    return NotFound();

                existing.Title = model.Title;
                existing.Description = model.Description;

                if (model.BannerFile != null && model.BannerFile.Length > 0)
                {
                    var imagePath = await FileUploadHelper.SaveFileAsync(model.BannerFile, "service");
                    existing.Img = imagePath;
                }

            }
            else
            {
                // CREATE
                string imagePath = "";
                if (model.BannerFile != null && model.BannerFile.Length > 0)
                {
                    imagePath = await FileUploadHelper.SaveFileAsync(model.BannerFile, "service");
                }
                else
                {
                    ModelState.AddModelError("BannerFile", "Please upload an image.");
                    ViewBag.ServicesList = await _context.Services.ToListAsync();
                    return View(model);
                }

                var newService = new Service
                {
                    Id = Guid.NewGuid(),
                    Title = model.Title,
                    Description = model.Description,
                    Img = imagePath
                };

                _context.Services.Add(newService);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Service");
        }

        // DELETE - Service
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("admin/services/delete/{id}")]
        public async Task<IActionResult> DeleteService(Guid id)
        {
            var user = await GetAdminUser();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var service = await _context.Services.FindAsync(id);
            if (service == null)
                return NotFound();

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            return RedirectToAction("Service");
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

        // GET: Gallery
        [HttpGet("admin/gallery")]
        public async Task<IActionResult> Gallery(string name, string type, Guid? id)
        {
            var user = await GetAdminUser();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var query = _context.Gallery.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                query = query.Where(g => g.Name.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                type = type.Trim();
                query = query.Where(g => g.Type == type);
            }

            var galleryList = await query.ToListAsync();

            Gallery selectedGallery = null;
            if (id.HasValue)
            {
                selectedGallery = await _context.Gallery.FirstOrDefaultAsync(g => g.Id == id.Value);
            }

            ViewBag.GalleryList = galleryList;
            ViewData["NameQuery"] = name;
            ViewData["TypeFilter"] = type;

            ModelState.Clear();

            return View(selectedGallery ?? new Gallery());
        }

        // CREATE AND UPDATE - Gallery
        [HttpPost("admin/gallery")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Gallery(Gallery model)
        {
            var user = await GetAdminUser();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrWhiteSpace(model.Name))
                ModelState.AddModelError("Name", "Name is required.");
            if (string.IsNullOrWhiteSpace(model.Description))
                ModelState.AddModelError("Description", "Description is required.");
            if (string.IsNullOrWhiteSpace(model.Type))
                ModelState.AddModelError("Type", "Type is required.");

            if (!ModelState.IsValid)
            {
                ViewBag.GalleryList = await _context.Gallery.ToListAsync();
                return View(model);
            }

            if (model.Id != Guid.Empty)
            {
                // UPDATE
                var existing = await _context.Gallery.FindAsync(model.Id);
                if (existing == null)
                    return NotFound();

                existing.Name = model.Name;
                existing.Description = model.Description;
                existing.Type = model.Type;

                if (model.BannerFile != null && model.BannerFile.Length > 0)
                {
                    var imagePath = await FileUploadHelper.SaveFileAsync(model.BannerFile, "gallery");
                    existing.Banner = imagePath;
                }
            }
            else
            {
                // CREATE
                string imagePath = "";
                if (model.BannerFile != null && model.BannerFile.Length > 0)
                {
                    imagePath = await FileUploadHelper.SaveFileAsync(model.BannerFile, "gallery");
                }
                else
                {
                    ModelState.AddModelError("BannerFile", "Please upload an image.");
                    ViewBag.GalleryList = await _context.Gallery.ToListAsync();
                    return View(model);
                }

                var newGallery = new Gallery
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    Description = model.Description,
                    Type = model.Type,
                    Banner = imagePath
                };

                _context.Gallery.Add(newGallery);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Gallery");
        }


        // DELETE - Gallery
        [HttpPost("admin/gallery/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGallery(Guid id)
        {
            var user = await GetAdminUser();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var gallery = await _context.Gallery.FindAsync(id);
            if (gallery == null)
                return NotFound();

            _context.Gallery.Remove(gallery);
            await _context.SaveChangesAsync();

            return RedirectToAction("Gallery");
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

            string token = _jwt.GenerateToken(user.Id.ToString(), user.Email);

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
