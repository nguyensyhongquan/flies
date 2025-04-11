using FliesProject.Data;
using FliesProject.Models.Entities;
using FliesProject.Repositories.IGenericRepository;
using FliesProject.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Shared;
using System.Security.Cryptography;
using System.Text;

namespace FliesProject.Controllers.Students
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly FiliesContext _dbContext;
        public AccountController(FiliesContext dbContext, IUserService userService)
        {
            _userService = userService;
            _dbContext = dbContext;
        }
        public IActionResult Home()
        {
            return View();
        }
        public IActionResult Dinosaur()
        {
            return View();
        }
        public IActionResult ProfileStudent()
        {
            var username = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Home", "Account");
            }

            // Assuming _userService is a service that fetches user data
            var user = _userService.GetUserByUsername(username);

            if (user == null)
            {
                return RedirectToAction("Home", "Account");
            }

            _dbContext.Entry(user)
                      .Collection(u => u.Courses)
            .Load();

            _dbContext.Entry(user)
                      .Collection(u => u.EnrollementStudents)
                      .Load();

            return View(user);
        }
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserName");

            return RedirectToAction("Home", "Account");
        }

        //Register code 
        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists
                var existingUsername = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
                if (existingUsername != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }

                // Check if email already exists
                var existingEmail = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng");
                    return View(model);
                }

                // Create new user
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    Passwordhash = HashPassword(model.Password),
                    Role = "student", // Default role is student
                    Status = "active",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    // Explicitly set nullable strings to empty to avoid potential UNIQUE NULL issues
                    Fullname = "", // Or null, depending on desired default
                    AvatarUrl = "", // Or null
                    Address = "",   // Or null
                    PhoneNumber = "" // Set to empty string instead of null
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Home", "Account");
            }

            return View(model);
        }

        // Helper method to hash password
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }


    }

}
