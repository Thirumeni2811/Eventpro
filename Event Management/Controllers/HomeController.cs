using Event_Management.Helpers;
using Event_Management.ViewModels;
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
                    user = null;
                }
            }

            var model = new HomeIndexViewModel();

            try
            {
                var galleryResponse = await _galleryService.GetAllAsync();
                model.Galleries = galleryResponse.Data?.ToList() ?? new List<Gallery>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching gallery data.");
            }

            try
            {
                var provideResponse = await _provideService.GetAllAsync();
                model.Provides = provideResponse.Data?.ToList() ?? new List<Provides>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching provides data.");
            }

            try
            {
                var servicesResponse = await _servService.GetAllAsync();
                model.Services = servicesResponse.Data?.ToList() ?? new List<Services>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching services data.");
            }

            model.Role = user?.Role;

            return View(model);
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