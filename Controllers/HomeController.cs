using System.Diagnostics;
using FliesProject.Models.Entities;
using FliesProject.Service;
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
                Console.WriteLine($"Received Username: {model.Username}");
                Console.WriteLine($"Received Raw Password: {model.Passwordhash}");

                // Tìm user theo username
                var user = _userService.GetUserByUsername(model.Username);
                if (user == null)
                {
                    return Json(new { success = false, message = "Invalid credentials" });
                }

                // Băm mật khẩu được gửi lên
                string hashedInputPassword = HashPassword(model.Passwordhash);

                Console.WriteLine($"Hashed Input Password: {hashedInputPassword}");
                Console.WriteLine($"Stored PasswordHash: {user.Passwordhash}");

                // So sánh mật khẩu đã băm
                if (!string.Equals(user.Passwordhash, hashedInputPassword, StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = "Invalid credentials" });
                }

                // Thiết lập session nếu tồn tại
                if (HttpContext.Session == null)
                {
                    return StatusCode(500, new { success = false, message = "Session is not available" });
                }

                HttpContext.Session.SetString("UserRole", user.Role ?? "");
                HttpContext.Session.SetString("UserName", user.Username ?? "");
                HttpContext.Session.SetString("UserAvatar", user.AvatarUrl ?? "");

                string homepageUrl = (user.Role ?? "").ToLower() switch
                {
                    "admin" => "/Admin/Home",
                    "mentor" => "/Mentor/Home",
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
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in Login: {ex}");
                return StatusCode(500, new { success = false, message = "Internal Server Error", error = ex.Message });
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower(); // Trả về chuỗi hex thường
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
