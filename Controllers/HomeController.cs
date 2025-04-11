using System.Diagnostics;
using FliesProject.Models;
using FliesProject.Models.Entities;
using FliesProject.Repositories.IGenericRepository;
using Microsoft.AspNetCore.Mvc;

namespace FliesProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserService _userService;

        public HomeController(IUserService userService)
        {
            _userService = userService;
        }
        public IActionResult Home()
        {
            return View();
        }
        public IActionResult Login([FromBody] User model)
        {
            try
            {
                Console.WriteLine($"Received JSON: {Newtonsoft.Json.JsonConvert.SerializeObject(model)}");
                Console.WriteLine($"Received Username: {model.Username}");
                Console.WriteLine($"Received Password: {model.Passwordhash}");

                var user = _userService.GetUserByUsername(model.Username);

                if (user != null && user.Passwordhash == model.Passwordhash)
                {
                    if (HttpContext.Session != null)
                    {
                        HttpContext.Session.SetString("UserRole", user.Role);
                        HttpContext.Session.SetString("UserName", user.Username);
                        HttpContext.Session.SetString("UserAvatar", user.AvatarUrl);

                        string homepageUrl = user.Role.ToLower() switch
                        {
                            "admin" => "/Admin/Home",
                            "mentor" => "/Course/Index",
                            "student" => "/Account/Home",
                            _ => "/Home"
                        };

                        return Json(new
                        {
                            success = true,
                            username = user.Username,
                            role = user.Role,
                            avatar = user.AvatarUrl,
                            homepageUrl = homepageUrl
                        });
                    }
                    else
                    {
                        return StatusCode(500, new { success = false, message = "Session is not available" });
                    }
                }

                return Json(new { success = false, message = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in Login: {ex}");
                return StatusCode(500, new { success = false, message = "Internal Server Error", error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult CheckLoginStatus()
        {
            var username = HttpContext.Session.GetString("UserName");
            if (!string.IsNullOrEmpty(username))
            {
                return Json(new { isLoggedIn = true, username = username });
            }
            return Json(new { isLoggedIn = false });
        }
    }
}
