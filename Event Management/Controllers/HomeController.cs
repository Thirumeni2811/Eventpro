using Event_Management.Helpers;
using Eventpro.Domain.Interfaces.IGallery;
using Eventpro.Domain.Interfaces.IProvide;
using Eventpro.Domain.Interfaces.IServ;
using Eventpro.Domain.Models;
using Eventpro.Service;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Event_Management.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IGalleryService _galleryService;
        private readonly IProvideService _provideService;
        private readonly IUserService _userService;
        private readonly IServService _servService;

        public HomeController(IGalleryService galleryService, IProvideService provideService, IServService servService, IUserService userService, ILogger<HomeController> logger)
        {
            _galleryService = galleryService;
            _provideService = provideService;
            _servService = servService;
            _userService = userService;

            _logger = logger;
        }

        private bool IsTokenValid()
        {
            return HttpContext.Request.Cookies.TryGetValue("token", out string? token) && !string.IsNullOrWhiteSpace(token);
        }

        public async Task<IActionResult> Index()
        {
            Guid userId;
            try
            {
                userId = TokenHelper.GetIdFromToken(Request);
            }
            catch
            {
                userId = Guid.Empty;
            }

            Users? user = null;
            if (userId != Guid.Empty)
            {
                try
                {
                    var userResult = await _userService.GetUserByIdAsync(userId);
                    if (userResult.Success && userResult.Data != null)
                    {
                        user = userResult.Data;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error retrieving user for userId {UserId}", userId);
                    // Fallback to null
                    user = null;
                }
            }

            IEnumerable<Gallery> galleries = Enumerable.Empty<Gallery>();
            IEnumerable<Provides> provides = Enumerable.Empty<Provides>();
            IEnumerable<Services> services = Enumerable.Empty<Services>();

            try
            {
                var galleryResponse = await _galleryService.GetAllAsync();
                galleries = galleryResponse.Data ?? Enumerable.Empty<Gallery>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching gallery data.");
            }

            try
            {
                var provideResponse = await _provideService.GetAllAsync();
                provides = provideResponse.Data ?? Enumerable.Empty<Provides>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching provides data.");
            }

            try
            {
                var servicesResponse = await _servService.GetAllAsync();
                services = servicesResponse.Data ?? Enumerable.Empty<Services>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching services data.");
            }

            ViewBag.Gallery = galleries.ToList();
            ViewBag.Provides = provides.ToList();
            ViewBag.Services = services.ToList();
            ViewBag.Role = user?.Role;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}