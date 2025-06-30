using Event_Management.Data;
using Event_Management.Helpers;
using Event_Management.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Event_Management.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly JwtHelper _jwt;

        public AccountController (AppDbContext context, JwtHelper jwt)
        {
            _context = context;
            _jwt = jwt;
        }

        // GET: /create-profile
        [HttpGet]
        [Route("/create-profile")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /create-profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/create-profile")]
        public async Task<IActionResult> Create(Users user)
        {
            if (string.IsNullOrWhiteSpace(user.Person))
                ModelState.AddModelError("Person", "Contact person name is required.");

            if (string.IsNullOrWhiteSpace(user.Address))
                ModelState.AddModelError("Address", "Address is required.");

            if (string.IsNullOrWhiteSpace(user.Password))
                ModelState.AddModelError(nameof(user.Password), "Password is required.");

            if (string.IsNullOrWhiteSpace(user.CPassword))
                ModelState.AddModelError(nameof(user.CPassword), "Please confirm your password.");

            if (!string.IsNullOrWhiteSpace(user.Password) && !string.IsNullOrWhiteSpace(user.CPassword))
            {
                if (user.Password != user.CPassword)
                    ModelState.AddModelError(nameof(user.CPassword), "Passwords do not match.");
            }

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            if (await _context.Users.AnyAsync(u => u.Email.ToLower().Trim() == user.Email.ToLower().Trim()))
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(user);
            }

            try
            {
                user.Password = PasswordHelper.HashPassword(user.Password);
                user.CPassword = null;
                user.Role = "Organizer";
                user.CreatedAt = DateTime.Now;

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                string token = _jwt.GenerateToken(user.Id.ToString(), user.Email);

                Response.Cookies.Append("Token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.Now.AddDays(7)
                });

                TempData["SuccessMessage"] = "Profile created successfully!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                return View(user);
            }
        }

        // GET: /org-login
        [HttpGet]
        [Route("/org-login")]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /org-login
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/org-login")]
        public async Task<IActionResult> Login(Login log)
        {
            if (!ModelState.IsValid)
            {
                return View(log);
            }

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower().Trim() == log.Email.ToLower().Trim() && u.Role == "Organizer");

                if (user == null)
                {
                    ModelState.AddModelError("Email", "Email does not exist as Organizer account.");
                    return View(log);
                }

                if (!PasswordHelper.VerifyPassword(log.Password, user.Password))
                {
                    ModelState.AddModelError("Password", "Incorrect password.");
                    return View(log);
                }

                string token = _jwt.GenerateToken(user.Id.ToString(), user.Email);

                Response.Cookies.Append("Token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.Now.AddDays(7)
                });

                TempData["SuccessMessage"] = "Login successful!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login Error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                return View(log);
            }
        }

        // GET: /update-org-profile
        [HttpGet]
        [Route("/update-org-profile")]
        public async Task <IActionResult> OrgUpdate()
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

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Clear password fields for editing
            user.Password = string.Empty;
            user.CPassword = string.Empty;

            return View(user);
        }

        // POST: /update-org-profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/update-org-profile")]
        public async Task<IActionResult> OrgUpdate(Users model, IFormFile ImageFile)
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

            var user = await _context.Users.FirstOrDefaultAsync(o => o.Id == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            ModelState.Remove("Password");
            ModelState.Remove("CPassword");
            ModelState.Remove("ImageFile");
            ModelState.Remove("Role");
            ModelState.Remove("CreatedAt");

            if (string.IsNullOrWhiteSpace(model.Person))
                ModelState.AddModelError("Person", "Contact person name is required.");

            if (string.IsNullOrWhiteSpace(model.Address))
                ModelState.AddModelError("Address", "Address is required.");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            user.Name = model.Name;
            user.Person = model.Person;
            user.Address = model.Address;
            user.Email = model.Email;
            user.PhoneNo = model.PhoneNo;
            user.Website = model.Website;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                try
                {
                    var imagePath = await FileUploadHelper.SaveFileAsync(ImageFile, "user");
                    user.Image = imagePath;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("Image", "Invalid image file.");
                    return View(model);
                }
            }
            else
            {
                user.Image = user.Image ?? model.Image;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("OrgEvents", "Organizer");
        }

        // U S E R

        // GET: /user-signup
        [HttpGet]
        [Route("/user-signup")]
        public IActionResult Signup()
        {
            return View();
        }

        // POST: /user-signup
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/user-signup")]
        public async Task<IActionResult> Signup(Users user)
        {
            ModelState.Remove("Person");
            ModelState.Remove("Address");
            ModelState.Remove("Role");

            if (string.IsNullOrWhiteSpace(user.Password))
                ModelState.AddModelError(nameof(user.Password), "Password is required.");

            if (string.IsNullOrWhiteSpace(user.CPassword))
                ModelState.AddModelError(nameof(user.CPassword), "Please confirm your password.");

            if (!string.IsNullOrWhiteSpace(user.Password) && !string.IsNullOrWhiteSpace(user.CPassword))
            {
                if (user.Password != user.CPassword)
                    ModelState.AddModelError(nameof(user.CPassword), "Passwords do not match.");
            }

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            if (await _context.Users.AnyAsync(u => u.Email.ToLower().Trim() == user.Email.ToLower().Trim()))
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(user);
            }

            try
            {
                user.Password = PasswordHelper.HashPassword(user.Password);
                user.CPassword = null;
                user.Role = "User";
                user.CreatedAt = DateTime.Now;

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                string token = _jwt.GenerateToken(user.Id.ToString(), user.Email);

                Response.Cookies.Append("Token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.Now.AddDays(7)
                });

                TempData["SuccessMessage"] = "User Profile created successfully!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                return View(user);
            }
        }


        // GET: /user-login
        [HttpGet]
        [Route("/user-login")]
        public IActionResult UserLogin()
        {
            return View();
        }

        // POST: /user-login
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/user-login")]
        public async Task<IActionResult> UserLogin(Login  data)
        {
            if (!ModelState.IsValid)
            {
                return View(data);
            }

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower().Trim() == data.Email.ToLower().Trim() && u.Role == "User");

                if (user == null)
                {
                    ModelState.AddModelError("Email", "Email does not exist as User account.");
                    return View(data);
                }

                if (!PasswordHelper.VerifyPassword(data.Password, user.Password))
                {
                    ModelState.AddModelError("Password", "Incorrect password.");
                    return View(data);
                }

                string token = _jwt.GenerateToken(user.Id.ToString(), user.Email);

                Response.Cookies.Append("Token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.Now.AddDays(7)
                });

                TempData["SuccessMessage"] = "Login successful!";
                
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login Error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                return View(data);
            }
        }

        // GET: /update-user-profile
        [HttpGet]
        [Route("/update-user-profile")]
        public async Task<IActionResult> UserUpdate()
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

            var user = await _context.Users
                .FirstOrDefaultAsync(o => o.Id == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Clear password fields for editing
            user.Password = string.Empty;
            user.CPassword = string.Empty;

            return View(user);
        }

        // POST: /update-user-profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/update-user-profile")]
        public async Task<IActionResult> UserUpdate(Users model, IFormFile ImageFile)
        {
            Guid userId;

            try
            {
                userId = TokenHelper.GetIdFromToken(Request);
            }
            catch
            {
                Console.WriteLine("TokenHelper failed. Redirecting.");
                return RedirectToAction("Create", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(o => o.Id == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            ModelState.Remove("Person");
            ModelState.Remove("Address");
            ModelState.Remove("Role");
            ModelState.Remove("Password");
            ModelState.Remove("CPassword");
            ModelState.Remove("ImageFile");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            user.Name = model.Name;
            user.Email = model.Email;
            user.PhoneNo = model.PhoneNo;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                try
                {
                    var imagePath = await FileUploadHelper.SaveFileAsync(ImageFile, "user");
                    user.Image = imagePath;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("Image", "Invalid image file.");
                    return View(model);
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Index", "Home");
        }

        // L O G O U T
        [HttpGet]
        [Route("/logout")]
        public IActionResult Logout()
        {
            var role = Request.Cookies["Role"];

            Response.Cookies.Delete("Token");
            Response.Cookies.Delete("Role");
            HttpContext.Session.Clear();

            if (role == "Organizer")
            {
                return RedirectToAction("Create", "Account");
            }
            else if (role == "User")
            {
                return RedirectToAction("Signup", "Account");
            }

            return RedirectToAction("Create", "Account");
        }
    }
}
