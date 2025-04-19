using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FliesProject.Models.Entities;
using FliesProject.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using FliesProject.Models.Entities.ViewModels;

namespace FliesProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        public HomeController(IUserService userService, IEmailService emailService)
        {
            _userService = userService;
            _emailService = emailService;
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

                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("UserRole", user.Role ?? "");
                HttpContext.Session.SetString("UserName", user.Username ?? "");
                HttpContext.Session.SetString("UserAvatar", user.AvatarUrl ?? "");

                // Thiết lập claims cho xác thực
                var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
    new Claim(ClaimTypes.Name, user.Username),
    new Claim(ClaimTypes.Role, user.Role ?? "")
};

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // Đặt true nếu muốn cookie lưu lâu dài sau khi đóng trình duyệt
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                };

                // Sử dụng phương thức đồng bộ thay vì await
                HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties).GetAwaiter().GetResult();

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
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        [HttpGet]
        public IActionResult CheckLoginStatus()
        {
            var username = HttpContext.Session.GetString("UserName");
            if (!string.IsNullOrEmpty(username))
            {
                var user = _userService.GetUserByUsername(username);
                return Json(new
                {
                    isLoggedIn = true,
                    username = username,
                    avatar = user.AvatarUrl
                });
            }

            return Json(new { isLoggedIn = false });
        }
        [HttpPost]
        public IActionResult SendResetLink([FromBody] string email)
        {
            try
            {
                var user = _userService.GetUserByEmail(email);
                if (user == null)
                {
                    return Json(new { success = false, message = "Email không tồn tại trong hệ thống." });
                }

                // Tạo token chứa UserId và timestamp (UTC ticks)
                var timestamp = DateTime.UtcNow.Ticks;
                var rawToken = $"{user.UserId}:{timestamp}";
                var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawToken));

                var resetLink = Url.Action("ResetPassword", "Account", new { token = encodedToken }, Request.Scheme);

                string subject = "Yêu cầu đặt lại mật khẩu";
                string body = $@"
            <p>Chào {user.Username},</p>
            <p>Bạn vừa yêu cầu đặt lại mật khẩu. Vui lòng nhấn vào liên kết bên dưới để tiếp tục:</p>
            <p><a href='{resetLink}'>Đặt lại mật khẩu</a></p>
            <p><b>Lưu ý:</b> Liên kết sẽ hết hạn sau <b>5 phút</b>.</p>";

                _emailService.SendEmail(email, subject, body);

                return Json(new { success = true, message = "Đã gửi liên kết đặt lại mật khẩu đến email của bạn." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in SendResetLink: {ex}");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống", error = ex.Message });
            }
        }

    }
}