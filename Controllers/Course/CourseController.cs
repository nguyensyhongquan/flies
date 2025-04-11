using FliesProject.Data;
using FliesProject.Models;
using FliesProject.Models.Entities.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FliesProject.Controllers.Course
{
    public class CourseController : Controller
    {
        private readonly FiliesContext _context;

        public CourseController(FiliesContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == 1);
            var courses = _context.Courses.Where(c => c.CreatedBy == user.UserId).ToList();

            var viewModel = new CourseUserViewModel
            {
                User = user,
                Courses = courses
            };

            ViewData["UserAvatar"] = user?.AvatarUrl;
            return View(viewModel);
        }

        public ActionResult Details(int id)
        {
            return View();
        }

        public ActionResult Create()
        {
            return View("CreateCourse");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FliesProject.Models.Entities.Course course, IFormFile ImageFile)
        {
            Console.WriteLine("Create action called.");
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid.");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"Field: {error.Key}");
                    foreach (var err in error.Value.Errors)
                    {
                        Console.WriteLine($"Error: {err.ErrorMessage}");
                    }
                }
                return View("CreateCourse", course);
            }

            try
            {
                Console.WriteLine($"ImageFile: {(ImageFile != null ? ImageFile.FileName : "null")}");
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(ImageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ImageFile", "Only image files (.jpg, .jpeg, .png, .gif) are allowed.");
                        return View("CreateCourse", course);
                    }

                    const int maxFileSize = 5 * 1024 * 1024; // 5MB
                    if (ImageFile.Length > maxFileSize)
                    {
                        ModelState.AddModelError("ImageFile", "File size cannot exceed 5MB.");
                        return View("CreateCourse", course);
                    }

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                        Console.WriteLine($"Created directory: {uploadsFolder}");
                    }

                    var fileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                        Console.WriteLine($"Saved file to: {filePath}");
                    }

                    course.CoursesPicture = "/Uploads/" + fileName;
                }

                course.CreatedAt = DateTime.Now;
                course.CreatedBy = 1;

                Console.WriteLine($"Course data: Title={course.Title}, Price={course.Price}, Timelimit={course.Timelimit}, Picture={course.CoursesPicture}");
                _context.Courses.Add(course);
                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"Saved {result} record(s) to database.");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Error saving course: {ex.Message}");
                return View("CreateCourse", course);
            }
        }

        public ActionResult Edit(int id)
        {
            return View();
        }

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

        public ActionResult Delete(int id)
        {
            return View();
        }

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