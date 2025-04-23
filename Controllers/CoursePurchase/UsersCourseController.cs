using FliesProject.Data;
using FliesProject.Models.Entities;
using FliesProject.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Shared;
using MySqlX.XDevAPI;
using System.Security.Claims;

namespace FliesProject.Controllers.CoursePurchase
{
    public class UsersCourseController : Controller
    {
        private readonly FiliesContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsersCourseController(FiliesContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult CourseOverview()
        {
            var courses = _dbContext.Courses.ToList(); // Assuming you have DbContext injected
            return View(courses);
        }

        // Action hiển thị popup xác nhận mua khóa học
        public async Task<IActionResult> ConfirmPurchase(int id)
        {
            // Kiểm tra đăng nhập
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home", new { returnUrl = Url.Action("CourseDescribe", "UsersCourse", new { id }) });
            }

            // Lấy thông tin khóa học
            var course = await _dbContext.Courses
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            // Kiểm tra xem người dùng đã mua khóa học chưa
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isEnrolled = await _dbContext.Enrollements
                .AnyAsync(e => e.CourseId == id && e.StudentId == userId);

            if (isEnrolled)
            {
                TempData["Message"] = "Bạn đã đăng ký khóa học này rồi!";
                return RedirectToAction("CourseDescribe", new { id });
            }

            // Trả về partial view cho modal xác nhận
            return PartialView("_ConfirmPurchasePartial", course);
        }

        // Action xử lý mua khóa học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase(int id)
        {
            // Kiểm tra đăng nhập
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }

            // Lấy thông tin người dùng hiện tại
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _dbContext.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Lấy thông tin khóa học
            var course = await _dbContext.Courses
                .Include(c => c.CreatedByNavigation)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            // Kiểm tra số dư
            if (user.Balance < course.Price)
            {
                TempData["ErrorMessage"] = "Balance is not enough to handle this transaction";
                return RedirectToAction("CourseDescribe", new { id });
            }

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Tạo Enrollment mới
                    var enrollment = new Enrollement
                    {
                        CourseId = course.CourseId,
                        StudentId = userId,
                        MentorId = course.CreatedBy,
                        StartedAt = DateTime.Now,
                        LimitedAt = course.Timelimit.HasValue ? DateTime.Now.AddDays(course.Timelimit.Value) : null,
                        Status = "Active"
                    };

                    _dbContext.Enrollements.Add(enrollment);
                    await _dbContext.SaveChangesAsync();

                    // Tạo CourseTransaction
                    var courseTransaction = new CourseTransaction
                    {
                        EnrollementId = enrollment.EnrollementId,
                        Amount = course.Price ?? 0,
                        TransactionDate = DateTime.Now
                    };

                    _dbContext.CourseTransactions.Add(courseTransaction);

                    // Trừ tiền từ tài khoản người dùng
                    user.Balance -= course.Price;
                    _dbContext.Users.Update(user);

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Buy Course Successfull!";
                    return RedirectToAction("CourseDescribe", new { id });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi thanh toán: " + ex.Message;
                    return RedirectToAction("CourseDescribe", new { id });
                }
            }
        }

        // Các action khác giữ nguyên
        public async Task<IActionResult> CourseDescribe(int id)
        {
            var course = await _dbContext.Courses
                .Include(c => c.CreatedByNavigation)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            // Kiểm tra xem người dùng đã đăng ký khóa học chưa
            bool isEnrolled = false;
            if (User.Identity.IsAuthenticated)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                isEnrolled = await _dbContext.Enrollements
                    .AnyAsync(e => e.CourseId == id && e.StudentId == userId);
            }

            ViewBag.IsEnrolled = isEnrolled;

            return View(course);
        }

        [HttpGet]
        public async Task<IActionResult> CourseDetail(int id, int lessonId)
        {
            // Lấy thông tin người dùng hiện tại
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Lấy thông tin khóa học, section và lesson
            var course = await _dbContext.Courses
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            // Kiểm tra xem người dùng có được ghi danh vào khóa học không
            var enrollment = await _dbContext.Enrollements
                .FirstOrDefaultAsync(e => e.CourseId == id && e.StudentId == userId);

            if (enrollment == null)
            {
                return Forbid();
            }

            // If lessonId is not provided, get the first lesson from the course
            if (lessonId == 0 && course.Sections.Any() && course.Sections.First().Lessons.Any())
            {
                lessonId = course.Sections.First().Lessons.First().LessonId;
            }


            // Lấy thông tin tiến độ khóa học của người dùng
            var courseProgress = await _dbContext.UserCourseProgresses
                .FirstOrDefaultAsync(p => p.EnrollementId == enrollment.EnrollementId);

            // Nếu chưa có tiến độ, tạo mới
            if (courseProgress == null)
            {
                int totalLessons = course.Sections.Sum(s => s.Lessons.Count);
                int totalQuizzes = await _dbContext.LessonQuizMappings
                          .Where(lq => lq.Lesson.Section.CourseId == id)
                        .Select(lq => lq.QuizId)
                  
                           .CountAsync();

                courseProgress = new UserCourseProgress
                {
                    EnrollementId = enrollment.EnrollementId,
                    CompletedLessons = 0,
                     CompletedQuizzes = 0,
                    TotalLessons = totalLessons,
                     TotalQuizzes = totalQuizzes,
                    ProgressPercentage = 0,
                    UpdatedAt = DateTime.Now
                };

                _dbContext.UserCourseProgresses.Add(courseProgress);
                await _dbContext.SaveChangesAsync();
            }

            // Lấy danh sách các bài học đã hoàn thành
            var completedLessons = await _dbContext.LessonCompletions
                .Where(lc => lc.EnrollementId == enrollment.EnrollementId)
                .Select(lc => lc.LessonId)
                .ToListAsync();

            var completedLessonsDict = completedLessons.ToDictionary(id => id, id => true);

            // Lấy danh sách các quiz đã hoàn thành
            var completedQuizzes = await _dbContext.QuizCompletions
                .Where(qc => qc.EnrollementId == enrollment.EnrollementId)
                .Select(qc => qc.QuizId)
                .ToListAsync();

            var completedQuizzesDict = completedQuizzes.ToDictionary(id => id, id => true);





            // Get current user's avatar
            var currentUser = await _dbContext.Users.FindAsync(userId);

            // Fetch comments for the specific lesson
            var comments = await _dbContext.QuizComments
                .Where(c => c.LessonId == lessonId && c.ParentCommentId == null)
                .Include(c => c.User)
                .Include(c => c.InverseParentComment)
                    .ThenInclude(r => r.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            // Get all lesson quiz mappings for this course
            var allLessonIds = course.Sections.SelectMany(s => s.Lessons).Select(l => l.LessonId).ToList();
            

            var lessonQuizMappings = await _dbContext.LessonQuizMappings
                .Where(lq => allLessonIds.Contains(lq.LessonId))
                .Include(lq => lq.Quiz)
                .ToListAsync();

            // Group quizzes by lesson ID
            var lessonQuizzes = new Dictionary<int, List<Quiz>>();
            foreach (var mapping in lessonQuizMappings)
            {
                if (!lessonQuizzes.ContainsKey(mapping.LessonId))
                {
                    lessonQuizzes[mapping.LessonId] = new List<Quiz>();
                }

                lessonQuizzes[mapping.LessonId].Add(mapping.Quiz);
            }

            // Tạo view model để truyền cho view
            var viewModel = new CourseDetailViewModel
            {
                Course = course,
                UserId = userId,
                MentorId = course.CreatedBy,
                Enrollement = enrollment,
                CourseProgress = courseProgress,
                CompletedLessonIds = completedLessonsDict,
                CompletedQuizIds = completedQuizzesDict,
                LessonQuizzes = lessonQuizzes,
                // Comment related properties
                Comments = comments,
                CurrentLessonId = lessonId,
                CurrentUserAvatar = currentUser?.AvatarUrl
            };


            // Trả về dữ liệu cho view
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> MarkLessonComplete(int lessonId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Lấy thông tin bài học
            var lesson = await _dbContext.Lessons
                .Include(l => l.Section)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);

            if (lesson == null)
            {
                return NotFound();
            }

            // Lấy thông tin ghi danh của người dùng
            var enrollment = await _dbContext.Enrollements
                .FirstOrDefaultAsync(e => e.CourseId == lesson.Section.CourseId && e.StudentId == userId);

            if (enrollment == null)
            {
                return Forbid();
            }

            // Kiểm tra xem bài học đã được đánh dấu hoàn thành chưa
            var existingCompletion = await _dbContext.LessonCompletions
                .FirstOrDefaultAsync(lc => lc.EnrollementId == enrollment.EnrollementId && lc.LessonId == lessonId);

            UserCourseProgress courseProgress = null;


            if (existingCompletion == null)
            {
                // Thêm mới nếu chưa có
                var lessonCompletion = new LessonCompletion
                {
                    EnrollementId = enrollment.EnrollementId,
                    LessonId = lessonId,
                    CompletedAt = DateTime.Now
                };

                _dbContext.LessonCompletions.Add(lessonCompletion);

                // Cập nhật tiến độ khóa học
                courseProgress = await _dbContext.UserCourseProgresses
                   .FirstOrDefaultAsync(p => p.EnrollementId == enrollment.EnrollementId);

                if (courseProgress != null)
                {
                    courseProgress.CompletedLessons = (courseProgress.CompletedLessons ?? 0) + 1;

                    // Tính toán phần trăm hoàn thành
                    decimal totalItems = courseProgress.TotalLessons + courseProgress.TotalQuizzes;
                    decimal completedItems = (courseProgress.CompletedLessons ?? 0) + (courseProgress.CompletedQuizzes ?? 0);
                    courseProgress.ProgressPercentage = (completedItems / totalItems) * 100;
                    courseProgress.UpdatedAt = DateTime.Now;
                }

                await _dbContext.SaveChangesAsync();
            }
            else
            {
                // Nếu đã hoàn thành rồi, chỉ cần lấy thông tin tiến độ hiện tại
                courseProgress = await _dbContext.UserCourseProgresses
                    .FirstOrDefaultAsync(p => p.EnrollementId == enrollment.EnrollementId);
            }

            // Kiểm tra nếu là AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // Trả về JSON response với dữ liệu tiến độ
                return Json(new
                {
                    success = true,
                    message = "Lesson marked as complete",
                    progressData = new
                    {
                        progressPercentage = courseProgress?.ProgressPercentage,
                        completedLessons = courseProgress?.CompletedLessons,
                        totalLessons = courseProgress?.TotalLessons,
                        completedQuizzes = courseProgress?.CompletedQuizzes,
                        totalQuizzes = courseProgress?.TotalQuizzes
                    }
                });
            }

            // Chuyển hướng đến trang chi tiết khóa học
            return RedirectToAction("CourseDetail", new { id = lesson.Section.CourseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int lessonId, string commentText, int? parentCommentId)
        {
            if (string.IsNullOrWhiteSpace(commentText))
            {
                return RedirectToAction("CourseDetail", new { id = GetCourseIdFromLessonId(lessonId), lessonId = lessonId });
            }

            // Get current user
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Create new comment
            var comment = new QuizComment
            {
                LessonId = lessonId,
                UserId = userId,
                CommentText = commentText,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.Now
            };

            // Add and save
            _dbContext.QuizComments.Add(comment);
            await _dbContext.SaveChangesAsync();

            // Get the course ID from lesson ID
            var courseId = await GetCourseIdFromLessonId(lessonId);

            // Redirect back to course detail with the current lesson ID
            return RedirectToAction("CourseDetail", new { id = courseId, lessonId = lessonId });
        }

        // Helper method to get course ID from lesson ID
        private async Task<int> GetCourseIdFromLessonId(int lessonId)
        {
            var lesson = await _dbContext.Lessons
                .Include(l => l.Section)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);

            if (lesson != null && lesson.Section != null)
            {
                return lesson.Section.CourseId;
            }

            return 0; // Or handle this case appropriately
        }
    }
}
