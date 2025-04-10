using FliesProject.Data;
using FliesProject.Service;
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

            var user = _userService.GetUserByUsername(username);

            if (user == null)
            {
                return RedirectToAction("Home", "Account");
            }

            _dbContext.Entry(user)
                      .Collection(u => u.EnrollementStudents)
                      .Query()
                      .Include(e => e.Course) // Include để lấy thông tin khóa học
                      .Load();

            // Lấy title của khóa học có StartedAt mới nhất trong các Enrollment
            var latestCourseTitle = user.EnrollementStudents
                .Where(e => e.Course != null)
                .OrderByDescending(e => e.StartedAt)
                .FirstOrDefault()?.Course?.Title;

            ViewBag.LatestCourseTitle = latestCourseTitle;

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
