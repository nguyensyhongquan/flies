    using FliesProject.Data;
using FliesProject.Models.Entities;
using Microsoft.EntityFrameworkCore;
    using FliesProject.Models.Entities.ViewModels;
    using FliesProject.Repositories.IGenericRepository;
    using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    namespace FliesProject.Controllers.Course
    {
        public class CourseController : Controller
        {
            private readonly IUserRepository _userRepository;
            private readonly FiliesContext _context;

            public CourseController(IUserRepository userRepository, FiliesContext context)
            {
                _userRepository = userRepository;
                _context = context;
            }

            public async Task<IActionResult> Index()
            {
                // Lấy UserId từ Session
                var userIdStr = HttpContext.Session.GetString("UserId");
                var userRole = HttpContext.Session.GetString("UserRole")?.ToLower();

                // Kiểm tra đăng nhập và vai trò Mentor
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("Vui lòng đăng nhập.");
                }

                if (userRole != "mentor")
                {
                    return Forbid("Chỉ Mentor mới có quyền truy cập.");
                }

                // Lấy thông tin user từ repository
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng.");
                }

                // Lấy danh sách khóa học của user
                var courses = _context.Courses
                    .Where(c => c.CreatedBy == user.UserId)
                    .ToList();

                // Tạo view model
                var viewModel = new CourseUserViewModel
                {
                    User = user,
                    Courses = courses
                };

                ViewData["UserAvatar"] = user.AvatarUrl;
                return View(viewModel);
            }

            public IActionResult Create()
            {
                // Kiểm tra đăng nhập và vai trò
                var userIdStr = HttpContext.Session.GetString("UserId");
                var userRole = HttpContext.Session.GetString("UserRole")?.ToLower();

                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out _))
                {
                    return Unauthorized("Vui lòng đăng nhập.");
                }

                if (userRole != "mentor")
                {
                    return Forbid("Chỉ Mentor mới có quyền tạo khóa học.");
                }

                return View("CreateCourse");
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FliesProject.Models.Entities.Course course, IFormFile? ImageFile) // Simplified to Course with correct namespace
        {
            Console.WriteLine("Bắt đầu xử lý Create action.");

            var userIdStr = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole")?.ToLower();

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                Console.WriteLine("Lỗi: UserId không hợp lệ hoặc chưa đăng nhập.");
                return Unauthorized("Vui lòng đăng nhập.");
            }

            Console.WriteLine($"UserId: {userId}, Role: {userRole}");

            if (userRole != "mentor")
            {
                Console.WriteLine("Lỗi: Người dùng không phải Mentor.");
                return Forbid("Chỉ Mentor mới có quyền tạo khóa học.");
            }

            // Validate CreatedBy exists
            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            Console.WriteLine($"Kiểm tra user tồn tại: {userExists}");
            if (!userExists)
            {
                ModelState.AddModelError("", "Người dùng không tồn tại.");
                Console.WriteLine("Lỗi: UserId không tồn tại trong Users.");
                return View("CreateCourse", course);
            }

            // Log form data
            Console.WriteLine($"Form data: Title={course.Title}, Description={course.Description}, Price={course.Price}, Timelimit={course.Timelimit}, CoursesPicture={course.CoursesPicture}, ImageFile={(ImageFile != null ? ImageFile.FileName : "null")}");

            // Remove CreatedByNavigation validation error
            ModelState.Remove("CreatedByNavigation");
            ModelState.Remove("ImageFile");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
                Console.WriteLine("ModelState không hợp lệ: " + string.Join("; ", errors));
                foreach (var key in ModelState.Keys)
                {
                    var entry = ModelState[key];
                    if (entry.Errors.Any())
                    {
                        Console.WriteLine($"Field {key}: {string.Join("; ", entry.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
                return View("CreateCourse", course);
            }

            try
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    Console.WriteLine($"Xử lý file ảnh: {ImageFile.FileName}");
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(ImageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        Console.WriteLine("Lỗi: Định dạng file không hợp lệ.");
                        ModelState.AddModelError("ImageFile", "Chỉ cho phép các file ảnh (.jpg, .jpeg, .png, .gif).");
                        return View("CreateCourse", course);
                    }

                    const int maxFileSize = 5 * 1024 * 1024; // 5MB
                    if (ImageFile.Length > maxFileSize)
                    {
                        Console.WriteLine("Lỗi: File quá lớn.");
                        ModelState.AddModelError("ImageFile", "Kích thước file không được vượt quá 5MB.");
                        return View("CreateCourse", course);
                    }

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                    Directory.CreateDirectory(uploadsFolder);
                    var fileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                        Console.WriteLine($"Đã lưu file tại: {filePath}");
                    }

                    course.CoursesPicture = "/Uploads/" + fileName;
                }
                else
                {
                    Console.WriteLine("Không có file ảnh, gán CoursesPicture = null.");
                    course.CoursesPicture = null;
                }

                course.CreatedAt = DateTime.Now;
                course.CreatedBy = userId;

                Console.WriteLine($"Chuẩn bị lưu khóa học: Title={course.Title}, Price={course.Price}, Timelimit={course.Timelimit}, CreatedBy={course.CreatedBy}, CreatedAt={course.CreatedAt}");
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                Console.WriteLine("Lưu khóa học thành công.");

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine($"DbUpdateException: {errorMessage}");
                ModelState.AddModelError("", $"Lỗi khi lưu khóa học: {errorMessage}");
                return View("CreateCourse", course);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}\nStackTrace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Lỗi khi lưu khóa học: {ex.Message}");
                return View("CreateCourse", course);
            }
        }

        public async Task<IActionResult> Edit(int id)
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                var userRole = HttpContext.Session.GetString("UserRole")?.ToLower();

                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("Vui lòng đăng nhập.");
                }

                if (userRole != "mentor")
                {
                    return Forbid("Chỉ Mentor mới có quyền chỉnh sửa khóa học.");
                }

                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                {
                    return NotFound("Khóa học không tồn tại.");
                }

                if (course.CreatedBy != userId)
                {
                    return Forbid("Bạn không có quyền chỉnh sửa khóa học này.");
                }

                return View("EditCourse", course);
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, FliesProject.Models.Entities.Course course, IFormFile ImageFile)
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                var userRole = HttpContext.Session.GetString("UserRole")?.ToLower();

                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("Vui lòng đăng nhập.");
                }

                if (userRole != "mentor")
                {
                    return Forbid("Chỉ Mentor mới có quyền chỉnh sửa khóa học.");
                }

                if (id != course.CourseId)
                {
                    return BadRequest("ID không hợp lệ.");
                }

                var existingCourse = await _context.Courses.FindAsync(id);
                if (existingCourse == null)
                {
                    return NotFound("Khóa học không tồn tại.");
                }

                if (existingCourse.CreatedBy != userId)
                {
                    return Forbid("Bạn không có quyền chỉnh sửa khóa học này.");
                }

                if (!ModelState.IsValid)
                {
                    return View("EditCourse", course);
                }

                try
                {
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var fileExtension = Path.GetExtension(ImageFile.FileName).ToLower();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("ImageFile", "Only image files (.jpg, .jpeg, .png, .gif) are allowed.");
                            return View("EditCourse", course);
                        }

                        const int maxFileSize = 5 * 1024 * 1024; // 5MB
                        if (ImageFile.Length > maxFileSize)
                        {
                            ModelState.AddModelError("ImageFile", "File size cannot exceed 5MB.");
                            return View("EditCourse", course);
                        }

                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var fileName = Guid.NewGuid().ToString() + fileExtension;
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(existingCourse.CoursesPicture))
                        {
                            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingCourse.CoursesPicture.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        existingCourse.CoursesPicture = "/Uploads/" + fileName;
                    }

                    // Cập nhật thông tin khóa học
                    existingCourse.Title = course.Title;
                    existingCourse.Description = course.Description;
                    existingCourse.Price = course.Price;
                    existingCourse.Timelimit = course.Timelimit;
                    existingCourse.CreatedAt = DateTime.Now;

                    _context.Courses.Update(existingCourse);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    ModelState.AddModelError("", $"Error updating course: {ex.Message}");
                    return View("EditCourse", course);
                }
            }

            public async Task<IActionResult> Delete(int id)
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                var userRole = HttpContext.Session.GetString("UserRole")?.ToLower();

                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("Vui lòng đăng nhập.");
                }

                if (userRole != "mentor")
                {
                    return Forbid("Chỉ Mentor mới có quyền xóa khóa học.");
                }

                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                {
                    return NotFound("Khóa học không tồn tại.");
                }

                if (course.CreatedBy != userId)
                {
                    return Forbid("Bạn không có quyền xóa khóa học này.");
                }

                return View("DeleteCourse", course);
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Delete(int id, IFormCollection collection)
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                var userRole = HttpContext.Session.GetString("UserRole")?.ToLower();

                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("Vui lòng đăng nhập.");
                }

                if (userRole != "mentor")
                {
                    return Forbid("Chỉ Mentor mới có quyền xóa khóa học.");
                }

                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                {
                    return NotFound("Khóa học không tồn tại.");
                }

                if (course.CreatedBy != userId)
                {
                    return Forbid("Bạn không có quyền xóa khóa học này.");
                }

                try
                {
                    // Xóa ảnh nếu có
                    if (!string.IsNullOrEmpty(course.CoursesPicture))
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", course.CoursesPicture.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }

                    _context.Courses.Remove(course);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    ModelState.AddModelError("", $"Error deleting course: {ex.Message}");
                    return View("DeleteCourse", course);
                }
            }

        // GET: Course/Manage/{id}
    
        public async Task<IActionResult> Show(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole")?.ToLower();

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                TempData["Error"] = "Please log in.";
                return RedirectToAction(nameof(Index));
            }

            if (userRole != "mentor")
            {
                TempData["Error"] = "Only mentors can manage courses.";
                return RedirectToAction(nameof(Index));
            }

            // Debug: List owned CourseIds
            var userCourses = await _context.Courses
                .Where(c => c.CreatedBy == userId)
                .Select(c => c.CourseId)
                .ToListAsync();
            TempData["DebugCourses"] = $"User owns courses: {string.Join(", ", userCourses)}";

            var course = await _context.Courses
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                .Select(c => new
                {
                    Course = c,
                    EnrollmentCount = c.Enrollements.Count()
                })
                .FirstOrDefaultAsync(c => c.Course.CourseId == id && c.Course.CreatedBy == userId);

            if (course == null)
            {
                TempData["Error"] = $"Course ID {id} not found or you don't have permission.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new CourseManageViewModel
            {
                CourseId = course.Course.CourseId,
                Title = course.Course.Title,
                EnrollmentCount = course.EnrollmentCount,
                Sections = course.Course.Sections
                    .OrderBy(s => s.Positition)
                    .Select(s => new SectionViewModel
                    {
                        SectionId = s.SectionId,
                        Title = s.Title,
                        Description = s.Description,
                        Position = s.Positition ?? 0,
                        Lessons = s.Lessons
                            .Select(l => new LessonViewModel
                            {
                                LessonId = l.LessonId,
                                Title = l.Title,
                                VideoUrl = l.VideoUrl,
                                Duration = l.Duration,
                                CreatedAt = l.CreatedAt
                            })
                            .ToList()
                    })
                    .ToList()
            };

            return View(viewModel);
        }

        // POST: Course/AddSection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSection(int courseId, string title, string description)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole")?.ToLower();

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                TempData["Error"] = "Please log in.";
                return RedirectToAction(nameof(Index));
            }

            if (userRole != "mentor")
            {
                TempData["Error"] = "Only mentors can add sections.";
                return RedirectToAction(nameof(Index));
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.CreatedBy == userId);

            if (course == null)
            {
                TempData["Error"] = "Course not found or you don't have permission.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["Error"] = "Section title is required.";
                return RedirectToAction(nameof(Show), new { id = courseId });
            }

            var maxPosition = await _context.Sections
                .Where(s => s.CourseId == courseId)
                .MaxAsync(s => (int?)s.Positition) ?? 0;

            var section = new Section
            {
                CourseId = courseId,
                Title = title,
                Description = description,
                Positition = maxPosition + 1,
                CreateAt = DateTime.Now
            };

            _context.Sections.Add(section);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Section added successfully.";
            return RedirectToAction(nameof(Show), new { id = courseId });
        }

        // POST: Course/AddLesson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLesson(int sectionId, string title, string videoUrl, int? duration)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole")?.ToLower();

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                TempData["Error"] = "Please log in.";
                return RedirectToAction(nameof(Index));
            }

            if (userRole != "mentor")
            {
                TempData["Error"] = "Only mentors can add lessons.";
                return RedirectToAction(nameof(Index));
            }

            var section = await _context.Sections
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.SectionId == sectionId);

            if (section == null || section.Course.CreatedBy != userId)
            {
                TempData["Error"] = "Section not found or you don't have permission.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(videoUrl))
            {
                TempData["Error"] = "Lesson title and video URL are required.";
                return RedirectToAction(nameof(Show), new { id = section.CourseId });
            }

            var lesson = new Lesson
            {
                SectionId = sectionId,
                Title = title,
                VideoUrl = videoUrl,
                Duration = duration,
                CreatedAt = DateTime.Now
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Lesson added successfully.";
            return RedirectToAction(nameof(Show), new { id = section.CourseId });
        }

        // GET: Course/DebugSession
        [HttpGet]
        public IActionResult DebugSession()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("UserRole");
            return Content($"UserId: {userId}, Role: {role}");
        }
    }
}
    