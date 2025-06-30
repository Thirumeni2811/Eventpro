//using System.Diagnostics;
//using Event_Management.Models;
//using Microsoft.AspNetCore.Mvc;
using Event_Management.Data;
using Event_Management.Helpers;
using Event_Management.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Event_Management.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private bool IsTokenValid()
        {
            return HttpContext.Request.Cookies.TryGetValue("token", out string? token) && !string.IsNullOrWhiteSpace(token);
        }
        public async Task<IActionResult> Index()
        {
            //if (!IsTokenValid())
            //    return RedirectToAction("Create", "Account");

            Guid userId;
            try
            {
                userId = TokenHelper.GetIdFromToken(Request);
            }
            catch
            {
                //return RedirectToAction("Create", "Account");
                userId = Guid.Empty;
            }

            //var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            //if (user == null)
            //{
            //    return RedirectToAction("Create", "Account");
            //}

            var user = userId != Guid.Empty
                ? await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
                : null;

            var galleries = await _context.Gallery.ToListAsync();
            var provides = await _context.Provides.ToListAsync();
            var services = await _context.Services.ToListAsync();

            var viewModel = new HomeView
            {
                Gallery = galleries,
                Provides = provides,
                Services = services,
                Role = user?.Role
            };

            return View(viewModel);
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