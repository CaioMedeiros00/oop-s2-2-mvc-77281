using Microsoft.AspNetCore.Mvc;
using Food.Domain.Models;
using Food.MVC.ViewModels;
using System.Diagnostics;

namespace Food.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Home page accessed");
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogError("Error page displayed. RequestId: {RequestId}", requestId);
            return View(new ErrorViewModel { RequestId = requestId });
        }
    }
}
