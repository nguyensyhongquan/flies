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
                      .Include(e => e.Course)
                      .Load();

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
        [HttpPost]
        public IActionResult UpdateProfile(string fullName, string birthday, string gender, string phone, string address)
        {
            var username = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(username))
            {
                return Json(new { success = false, message = "User not logged in" });
            }

            var user = _userService.GetUserByUsername(username);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Parse birthday (DateTime)
            DateTime parsedBirthday;
            if (!DateTime.TryParseExact(birthday, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out parsedBirthday))
            {
                return Json(new { success = false, message = "Invalid date format" });
            }

            user.Fullname = fullName;
            user.Birthday = parsedBirthday;
            user.Gender = gender;
            user.PhoneNumber = phone;
            user.Address = address;

            try
            {
                _dbContext.Users.Update(user);
                _dbContext.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Update failed: " + ex.Message });
            }
        }


    }

}
