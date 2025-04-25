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



        public async Task<IActionResult> Show()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole")?.ToLower();

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                TempData["Error"] = "Please log in.";
                return RedirectToAction("Index", "Home");
            }

            if (userRole != "mentor")
            {
                TempData["Error"] = "Only mentors can manage courses.";
                return RedirectToAction("Index", "Home");
            }

            var courses = await _context.Courses
                .Where(c => c.CreatedBy == userId)
                .Select(c => new CourseWithEnrollmentViewModel
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    CoursesPicture = c.CoursesPicture,
                    EnrollmentCount = c.Enrollements.Count()
                })
                .ToListAsync();

            return View("MyCourse", courses);
        }

        // GET: Course/Manage/{id}
        [HttpGet]
        public async Task<IActionResult> Manage(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole")?.ToLower();

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                TempData["Error"] = "Please log in.";
                return RedirectToAction("Show");
            }

            if (userRole != "mentor")
            {
                TempData["Error"] = "Only mentors can manage courses.";
                return RedirectToAction("Show");
            }

            var course = await _context.Courses
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                .FirstOrDefaultAsync(c => c.CourseId == id && c.CreatedBy == userId);

            if (course == null)
            {
                TempData["Error"] = "Course not found or you don't have permission.";
                return RedirectToAction("Show");
            }

            var model = new CourseManageViewModel
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Sections = course.Sections.Select(s => new SectionViewModel
                {
                    SectionId = s.SectionId,
                    Title = s.Title,
                    Description = s.Description,
                    Lessons = s.Lessons.Select(l => new LessonViewModel
                    {
                        LessonId = l.LessonId,
                        Title = l.Title,
                        LessonType = l.VideoUrl == "Quiz" ? "Quiz" : "Content",
                        VideoUrl = l.VideoUrl,
                        QuizIds = l.VideoUrl == "Quiz" ? _context.LessonQuizMappings
                            .Where(m => m.LessonId == l.LessonId)
                            .Select(m => m.QuizId)
                            .ToList() : null,
                        QuizTitles = l.VideoUrl == "Quiz" ? _context.LessonQuizMappings
                            .Include(m => m.Quiz)
                            .Where(m => m.LessonId == l.LessonId)
                            .Select(m => m.Quiz.Title)
                            .ToList() : null,
                        Duration = l.Duration,
                        CreatedAt = l.CreatedAt
                    }).ToList()
                }).ToList()
            };

            return View("Manage", model);
        }

        // GET: Course/AddSection/{courseId}
        [HttpGet]
        public async Task<IActionResult> AddSection(int courseId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole")?.ToLower();

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                TempData["Error"] = "Please log in.";
                return RedirectToAction("Show");
            }

            if (userRole != "mentor")
            {
                TempData["Error"] = "Only mentors can add sections.";
                return RedirectToAction("Show");
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.CreatedBy == userId);

            if (course == null)
            {
                TempData["Error"] = "Course not found or you don't have permission.";
                return RedirectToAction("Show");
            }

            var model = new AddSectionViewModel
            {
                CourseId = courseId,
                CourseTitle = course.Title
            };

            return View("AddSection", model);
        }

        // POST: Course/AddSection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSection(AddSectionViewModel model)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole")?.ToLower();

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                TempData["Error"] = "Please log in.";
                return RedirectToAction("Show");
            }

            if (userRole != "mentor")
            {
                TempData["Error"] = "Only mentors can add sections.";
                return RedirectToAction("Show");
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == model.CourseId && c.CreatedBy == userId);

            if (course == null)
            {
                TempData["Error"] = "Course not found or you don't have permission.";
                return RedirectToAction("Show");
            }

            if (string.IsNullOrWhiteSpace(model.Title))
            {
                TempData["Error"] = "Section title is required.";
                return View("AddSection", model);
            }

            var maxPosition = await _context.Sections
                .Where(s => s.CourseId == model.CourseId)
                .MaxAsync(s => (int?)s.Positition) ?? 0;

            var section = new Section
            {
                CourseId = model.CourseId,
                Title = model.Title,
                Description = model.Description,
                Positition = maxPosition + 1,
                CreateAt = DateTime.Now
            };

            _context.Sections.Add(section);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Section added successfully.";
            return RedirectToAction("Manage", new { id = model.CourseId });
        }

        // GET: Course/AddLesson/{courseId}
        [HttpGet]
        public async Task<IActionResult> AddLesson(int courseId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole")?.ToLower();

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                TempData["Error"] = "Please log in.";
                return RedirectToAction("Show");
            }

            if (userRole != "mentor")
            {
                TempData["Error"] = "Only mentors can add lessons.";
                return RedirectToAction("Show");
            }

            var course = await _context.Courses
                .Include(c => c.Sections)
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.CreatedBy == userId);

            if (course == null)
            {
                TempData["Error"] = "Course not found or you don't have permission.";
                return RedirectToAction("Show");
            }

            var model = new AddLessonViewModel
            {
                CourseId = courseId,
                CourseTitle = course.Title,
                Sections = course.Sections.Select(s => new SectionViewModel
                {
                    SectionId = s.SectionId,
                    Title = s.Title
                }).ToList(),
                Quizzes = await _context.Quizzes
                    .Select(q => new QuizViewModel
                    {
                        QuizId = q.QuizId,
                        Title = q.Title
                    })
                    .ToListAsync()
            };

            return View("AddLesson", model);
        }

        // POST: Course/AddLesson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLesson(AddLessonViewModel model, IFormFile? contentFile)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole")?.ToLower();
            Console.WriteLine("hiiiiiiiiiiiii");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                TempData["Error"] = "Please log in.";
                return RedirectToAction("Show");
            }

            if (userRole != "mentor")
            {
                TempData["Error"] = "Only mentors can add lessons.";
                return RedirectToAction("Show");
            }

            var section = await _context.Sections
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.SectionId == model.SectionId);

            if (section == null || section.Course.CreatedBy != userId)
            {
                TempData["Error"] = "Section not found or you don't have permission.";
                return RedirectToAction("Show");
            }

            if (string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.LessonType))
            {
                TempData["Error"] = "Lesson title and type are required.";
                await PopulateViewModel(model);
                return View("AddLesson", model);
            }

            if (model.LessonType != "Content" && model.LessonType != "Quiz")
            {
                TempData["Error"] = "Invalid lesson type.";
                await PopulateViewModel(model);
                return View("AddLesson", model);
            }

            var lesson = new Lesson
            {
                SectionId = model.SectionId,
                Title = model.Title,
                Duration = model.Duration,
                CreatedAt = DateTime.Now
            };

            try
            {
                if (model.LessonType == "Quiz")
                {
                    if (model.QuizIds == null || !model.QuizIds.Any() || !await _context.Quizzes.AnyAsync(q => model.QuizIds.Contains(q.QuizId)))
                    {
                        TempData["Error"] = "At least one valid quiz must be selected.";
                        await PopulateViewModel(model);
                        return View("AddLesson", model);
                    }
                    lesson.VideoUrl = "Quiz";

                }
                else if (contentFile != null && contentFile.Length > 0)
                {
                    Console.WriteLine("conhanfileeeeeeeeeeeeeee");
                    var allowedExtensions = new[] { ".mp4", ".mov", ".avi", ".pdf", ".doc", ".docx", ".txt" };
                    var fileExtension = Path.GetExtension(contentFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        TempData["Error"] = $"Only {string.Join(", ", allowedExtensions)} files are allowed.";
                        Console.WriteLine("=========loimodel");
                        await PopulateViewModel(model);
                        return View("AddLesson", model);
                    }

                    const int maxFileSize = 50 * 1024 * 1024; // 50MB
                    if (contentFile.Length > maxFileSize)
                    {
                        TempData["Error"] = "File size cannot exceed 50MB.";
                        await PopulateViewModel(model);
                        return View("AddLesson", model);
                    }

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Console.WriteLine("ditmemay loi roiupk dc");
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    else
                    {
                        Console.WriteLine("updauoc");
                    }

                    // Giữ tên file gốc, xử lý trùng lặp
                    var fileName = Path.GetFileNameWithoutExtension(contentFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, contentFile.FileName);
                    int counter = 1;

                    // Kiểm tra trùng lặp và thêm hậu tố nếu cần
                    while (System.IO.File.Exists(filePath))
                    {
                        fileName = $"{Path.GetFileNameWithoutExtension(contentFile.FileName)}_{counter}";
                        filePath = Path.Combine(uploadsFolder, $"{fileName}{fileExtension}");
                        counter++;
                    }

                    try
                    {
                        using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            await contentFile.CopyToAsync(stream);
                        }
                    }
                    catch (IOException ex)
                    {
                        TempData["Error"] = $"Failed to save file: {ex.Message}";
                        await PopulateViewModel(model);
                        return View("AddLesson", model);
                    }

                    lesson.VideoUrl = $"/Uploads/{Path.GetFileName(filePath)}";
                }
                else
                {
                    Console.WriteLine("url la "+contentFile.Name);
                    Console.WriteLine("doadai"+contentFile.Length);
                    Console.WriteLine("kbietnx");
                    TempData["Error"] = "A file is required for content-based lessons.";
                    await PopulateViewModel(model);
                    return View("AddLesson", model);
                }

                _context.Lessons.Add(lesson);
                await _context.SaveChangesAsync();

                if (model.LessonType == "Quiz")
                {
                    foreach (var quizId in model.QuizIds)
                    {
                        var mapping = new LessonQuizMapping
                        {
                            LessonId = lesson.LessonId,
                            QuizId = quizId
                        };
                        _context.LessonQuizMappings.Add(mapping);
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Lesson added successfully.";
                return RedirectToAction("Manage", new { id = section.CourseId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error adding lesson: {ex.Message}";
                await PopulateViewModel(model);
                return View("AddLesson", model);
            }
        }

        private async Task PopulateViewModel(AddLessonViewModel model)
        {
            model.Sections = await _context.Sections
                .Where(s => s.CourseId == model.CourseId)
                .Select(s => new SectionViewModel { SectionId = s.SectionId, Title = s.Title })
                .ToListAsync();
            model.Quizzes = await _context.Quizzes
                .Select(q => new QuizViewModel { QuizId = q.QuizId, Title = q.Title })
                .ToListAsync();
        }
        [HttpGet]
        public async Task<IActionResult> GetQuizDetails(int quizId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.QuizAnswers)
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.QuizWritingSamples)
                .FirstOrDefaultAsync(q => q.QuizId == quizId);

            if (quiz == null)
            {
                return NotFound();
            }

            var quizDetails = new QuizDetailsViewModel
            {
                Title = quiz.Title,
                Description = quiz.Content ?? "No description available",
                Questions = quiz.QuizQuestions?.Select(q => new QuizQuestionViewModel
                {
                    Text = q.QuestionText,
                    Type = q.QuestionType,
                    MediaUrl = q.MediaUrl,
                    Answers = q.QuizAnswers?.Select(a => new QuizAnswerViewModel
                    {
                        Text = a.AnswerText,
                        IsCorrect = a.IsCorrect
                    }).ToList() ?? new List<QuizAnswerViewModel>(),
                    WritingSamples = q.QuizWritingSamples?.Select(ws => new QuizWritingSampleViewModel
                    {
                        Sample = ws.SampleAnswer
                    }).ToList() ?? new List<QuizWritingSampleViewModel>()
                }).ToList() ?? new List<QuizQuestionViewModel>()
            };

            return Json(quizDetails);
        }

        // GET: Course/DebugSession (for testing)
        [HttpGet]
        public IActionResult DebugSession()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("UserRole");
            return Content($"UserId: {userId}, Role: {role}");
        }
    }
}


    