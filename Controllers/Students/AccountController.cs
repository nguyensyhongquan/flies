using Microsoft.AspNetCore.Mvc;

namespace FliesProject.Controllers.Students
{
    public class AccountController : Controller
    {
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
            return View();
        }
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserName");

            return RedirectToAction("Home", "Account");
        }
    }

}
