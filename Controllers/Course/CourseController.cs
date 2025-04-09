using FliesProject.Data;
using FliesProject.Models;
using FliesProject.Models.Entities.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FliesProject.Controllers.Course
{
    public class CourseController : Controller
    {

        private readonly FiliesContext _context;
        // GET: CourseController
        public CourseController(FiliesContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == 3); // test
            var courses = _context.Courses.Where(c => c.CreatedBy == user.UserId).ToList();

            var viewModel = new CourseUserViewModel
            {
                User = user,
                Courses = courses
            };

            return View(viewModel);
        }

        // GET: CourseController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CourseController/Create
        public ActionResult Create()
        {
            return View("CreateCourse");
        }

        // POST: CourseController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FliesProject.Models.Entities.Course course, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                // Handle image upload
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    course.CoursesPicture = "/uploads/" + fileName;
                }
                course.CreatedAt = DateTime.Now;
                course.CreatedBy = 1;

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
                return View("CreateCourse");
        }
        /* public ActionResult Create(IFormCollection collection)
         {
             try
             {
                 return RedirectToAction(nameof(Index));
             }
             catch
             {
                 return View();
             }
         }*/

        // GET: CourseController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CourseController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CourseController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CourseController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }


}
