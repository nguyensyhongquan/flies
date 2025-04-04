using System.Diagnostics;
<<<<<<< HEAD
using FliesProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace FliesProject.Controllers
=======
using FliesWebsite.Models;
using Microsoft.AspNetCore.Mvc;

namespace FliesWebsite.Controllers
>>>>>>> b503f00f649fa22ce292f2d9d69441ecf3dd73f2
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
            return View();
        }
<<<<<<< HEAD
        public IActionResult Index1()
        {
            return View();
        }
=======

>>>>>>> b503f00f649fa22ce292f2d9d69441ecf3dd73f2
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
