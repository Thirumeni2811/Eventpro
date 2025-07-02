using Event_Management.Helpers;
using Eventpro.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Event_Management.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController (IUserService userService)
        {
            _userService = userService;
        }

        /*--------------------------------------
                    C R E A T E
        --------------------------------------*/

        // O R G A N I Z E R

        [HttpGet]
        [Route("/create-profile")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/create-profile")]
        public async Task<IActionResult> Create(Users user)
        {
            user.Role = "Organizer";

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

            try
            {
                var result = await _userService.CreateUserProfileAsync(user);

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                    return View(user);
                }

                Response.Cookies.Append("Token", result.Data.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.Now.AddDays(30)
                });

                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating profile: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return View(user);
            }
        }


        // U S E R

        [HttpGet]
        [Route("/user-signup")]
        public IActionResult Signup()
        {
            return View();
        }

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

            try
            {
                user.Role = "User";

                var result = await _userService.CreateUserProfileAsync(user);

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                    return View(user);
                }

                Response.Cookies.Append("Token", result.Data.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.Now.AddDays(30)
                });

                TempData["SuccessMessage"] = "User profile created successfully!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user profile: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return View(user);
            }
        }

        /*--------------------------------------
                    L O G I N 
        --------------------------------------*/

        [HttpGet]
        [Route("/org-login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        [Route("/user-login")]
        public IActionResult UserLogin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/org-login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(nameof(email), "Email is required.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(nameof(password), "Password is required.");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            try
            {
                var result = await _userService.LoginOrganizerAsync(email, password);

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                    return View();
                }

                Response.Cookies.Append("Token", result.Data.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.Now.AddDays(30)
                });

                TempData["SuccessMessage"] = "Login successful!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login Error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/user-login")]
        public async Task<IActionResult> UserLogin(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(nameof(email), "Email is required.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(nameof(password), "Password is required.");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            try
            {
                var result = await _userService.LoginUserAsync(email, password);

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                    return View();
                }

                Response.Cookies.Append("Token", result.Data.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.Now.AddDays(30)
                });

                TempData["SuccessMessage"] = "Login successful!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login Error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                return View();
            }
        }


        /*--------------------------------------
                    U P D A T E 
        --------------------------------------*/

        [HttpGet]
        [Route("/update-profile")]
        public async Task<IActionResult> UpdateProfile()
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

            var result = await _userService.GetUserByIdAsync(userId);

            if (!result.Success || result.Data == null)
            {
                return NotFound(result.Message ?? "User not found.");
            }

            var user = result.Data;

            user.Password = string.Empty;
            user.CPassword = string.Empty;

            if (user.Role == "Organizer")
            {
                return View("OrgUpdate", user);
            }
            else
            {
                return View("UserUpdate", user);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/update-profile")]
        public async Task<IActionResult> UpdateProfile(Users model, IFormFile ImageFile)
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

            var userResult = await _userService.GetUserByIdAsync(userId);

            if (!userResult.Success || userResult.Data == null)
            {
                return NotFound(userResult.Message);
            }

            var userEntity = userResult.Data;

            if (userEntity == null)
            {
                return NotFound("User not found.");
            }

            ModelState.Remove("Role");
            ModelState.Remove("Password");
            ModelState.Remove("CPassword");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("ImageFile");

            if (userEntity.Role == "Organizer")
            {
                if (string.IsNullOrWhiteSpace(model.Person))
                    ModelState.AddModelError("Person", "Contact person name is required.");

                if (string.IsNullOrWhiteSpace(model.Address))
                    ModelState.AddModelError("Address", "Address is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (ImageFile != null && ImageFile.Length > 0)
            {
                try
                {
                    var imagePath = await Eventpro.Service.Helpers.FileUploadHelper.SaveFileAsync(ImageFile, "user");
                    model.Image = imagePath;
                }
                catch (Exception)
                {
                    ModelState.AddModelError("Image", "Invalid image file.");
                    return View(model);
                }
            }

            try
            {
                var result = await _userService.UpdateProfileAsync(
                    userId,
                    model,
                    userEntity.Role,
                    userId
                );

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                    return View(model);
                }

                TempData["SuccessMessage"] = "Profile updated successfully!";

                if (userEntity.Role == "Organizer")
                {
                    return RedirectToAction("OrgEvents", "Organizer");
                }
                else
                {
                    return RedirectToAction("UserEvent", "User");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating profile: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
                return View(model);
            }
        }

        /*--------------------------------------
                    L O G O U T
        --------------------------------------*/

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
