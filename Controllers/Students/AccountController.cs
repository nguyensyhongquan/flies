using FliesProject.Data;
using FliesProject.Repositories.IGenericRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    }

}
