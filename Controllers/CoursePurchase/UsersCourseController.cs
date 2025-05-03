using FliesProject.Data;
using FliesProject.Models.Entities;
using FliesProject.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Text.Json;

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
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Kiểm tra enrollment với điều kiện lessonId thuộc một section của course
            var enrollment = await _dbContext.Enrollements
                .Include(e => e.Course)
                    .ThenInclude(c => c.Sections)
                        .ThenInclude(s => s.Lessons)
                .FirstOrDefaultAsync(e => e.StudentId == userId
                    && e.Course.Sections.Any(s => s.Lessons.Any(l => l.LessonId == lessonId)));

            if (enrollment == null)
            {
                return Json(new { success = false, message = "Not enrolled in this course or lesson not found" });
            }

            var lessonCompletion = new LessonCompletion
            {
                EnrollementId = enrollment.EnrollementId,
                LessonId = lessonId,
                CompletedAt = DateTime.Now
            };
            _dbContext.LessonCompletions.Add(lessonCompletion);

            var courseProgress = await _dbContext.UserCourseProgresses
                .FirstOrDefaultAsync(p => p.EnrollementId == enrollment.EnrollementId);

            if (courseProgress != null)
            {
                courseProgress.CompletedLessons = await _dbContext.LessonCompletions
                    .CountAsync(lc => lc.EnrollementId == enrollment.EnrollementId);
                courseProgress.ProgressPercentage = (decimal?)CalculateProgressPercentage(
                    courseProgress.CompletedLessons,
                    courseProgress.TotalLessons,
                    courseProgress.CompletedQuizzes,
                    courseProgress.TotalQuizzes);
                courseProgress.UpdatedAt = DateTime.Now;
                _dbContext.UserCourseProgresses.Update(courseProgress);
            }

            await _dbContext.SaveChangesAsync();

            return Json(new
            {
                success = true,
                progressData = new
                {
                    progressPercentage = courseProgress?.ProgressPercentage ?? 0,
                    completedLessons = courseProgress?.CompletedLessons ?? 0,
                    totalLessons = courseProgress?.TotalLessons ?? 0,
                    completedQuizzes = courseProgress?.CompletedQuizzes ?? 0,
                    totalQuizzes = courseProgress?.TotalQuizzes ?? 0
                }
            });
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

        [HttpGet]
        public async Task<IActionResult> StartQuiz(int courseId, int quizId, int lessonId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Kiểm tra quyền truy cập khóa học
            var enrollment = await _dbContext.Enrollements
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == userId);

            if (enrollment == null)
            {
                return Forbid();
            }

            // Lấy thông tin quiz
            var quiz = await _dbContext.Quizzes
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.QuizAnswers)
                .FirstOrDefaultAsync(q => q.QuizId == quizId);

            if (quiz == null)
            {
                return NotFound();
            }

            var viewModel = new QuizViewModel
            {
                Quiz = quiz,
                CourseId = courseId,
                LessonId = lessonId
            };

            ViewBag.ShowQuiz = true;
            ViewBag.QuizViewModel = viewModel;

            // Lấy thông tin khóa học để hiển thị danh sách lesson
            var courseDetailViewModel = await GetCourseDetailViewModel(courseId, lessonId, userId);
            return View("CourseDetail", courseDetailViewModel);
        }

        // Action để nộp bài quiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuiz(QuizSubmitViewModel model)
        {
            if (model == null)
            {
                return BadRequest("Invalid quiz submission data");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Kiểm tra quyền truy cập
            var enrollment = await _dbContext.Enrollements
                .FirstOrDefaultAsync(e => e.CourseId == model.CourseId && e.StudentId == userId);

            if (enrollment == null)
            {
                return Forbid();
            }

            // Lấy thông tin quiz
            var quiz = await _dbContext.Quizzes
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.QuizAnswers)
                .FirstOrDefaultAsync(q => q.QuizId == model.QuizId);

            if (quiz == null)
            {
                return NotFound();
            }

              // Đảm bảo dictionary không null để tránh lỗi
    if (model.Answers == null) model.Answers = new Dictionary<int, string[]>();
    if (model.WritingAnswers == null) model.WritingAnswers = new Dictionary<int, string>();
    
    // Kiểm tra trùng lặp writing submission trước khi xử lý
    foreach (var question in quiz.QuizQuestions.Where(q => q.QuestionType.ToLower() == "writing"))
    {
                var existingSubmission = await _dbContext.QuizWritingSubmissions
                .Include(ws => ws.Question)
                .FirstOrDefaultAsync(ws => ws.QuestionId == question.QuestionId && ws.UserId == userId);

                if (existingSubmission != null)
                {
                    // Nếu đã nộp bài writing, truyền dữ liệu vào ViewBag
                    ViewBag.WritingSubmission = existingSubmission;
                    break;
                }
       }

    // Chấm điểm (bỏ qua câu hỏi writing)
    int totalQuestions = quiz.QuizQuestions.Count(q => q.QuestionType.ToLower() != "writing");
    int correctAnswers = 0;
    List<QuizWritingSubmission> writingSubmissions = new List<QuizWritingSubmission>();
    bool hasWritingSubmissions = false;
    
    // Dictionary để lưu thông tin chi tiết về câu trả lời
    Dictionary<int, bool> questionResultsDict = new Dictionary<int, bool>();
    Dictionary<int, string[]> userAnswersDict = new Dictionary<int, string[]>();

    foreach (var question in quiz.QuizQuestions)
    {
        // Xử lý câu hỏi writing
        if (question.QuestionType.ToLower() == "writing")
        {
            if (model.WritingAnswers.ContainsKey(question.QuestionId))
            {
                var submission = new QuizWritingSubmission
                {
                    QuestionId = question.QuestionId,
                    UserId = userId,
                    SubmissionText = model.WritingAnswers[question.QuestionId],
                    SubmittedAt = DateTime.Now
                };
                _dbContext.QuizWritingSubmissions.Add(submission);
                writingSubmissions.Add(submission);
                hasWritingSubmissions = true;
            }
            continue;
        }

                // Xử lý câu hỏi trắc nghiệm
                bool isCorrect = false;
                string[] userAnswerArray = new string[0];

                // Xử lý câu hỏi trắc nghiệm
                if (model.Answers.ContainsKey(question.QuestionId))
                {

                    userAnswerArray = model.Answers[question.QuestionId];
                    userAnswersDict[question.QuestionId] = userAnswerArray;

                    var userAnswerIds = userAnswerArray.Select(int.Parse).ToList();
                    var correctAnswerIds = question.QuizAnswers
                        .Where(a => a.IsCorrect)
                        .Select(a => a.AnswerId)
                        .ToList();

                    if (question.QuestionType.ToLower() == "multiple_choice")
                    {
                        if (userAnswerIds.OrderBy(id => id).SequenceEqual(correctAnswerIds.OrderBy(id => id)))
                        {
                            correctAnswers++;
                        }
                    }
                    else // true_false or single_choice
                    {
                        if (userAnswerIds.Count == 1 && correctAnswerIds.Contains(userAnswerIds[0]))
                        {
                            correctAnswers++;
                            isCorrect = true;
                        }
                    }
                }
                // Lưu kết quả của câu hỏi này
                questionResultsDict[question.QuestionId] = isCorrect;
            }

            // Tính điểm
            int score = totalQuestions > 0 ? (int)((correctAnswers / (double)totalQuestions) * 100) : 0;

            // Lưu bài writing submissions bất kể điểm số
            if (hasWritingSubmissions == true)
            {
                await _dbContext.SaveChangesAsync();

                // Truy vấn lại để lấy thông tin đầy đủ
                writingSubmissions = await _dbContext.QuizWritingSubmissions
                    .Include(w => w.Question)
                    .Where(w => w.UserId == userId &&
                        writingSubmissions.Select(ws => ws.QuestionId).Contains(w.QuestionId) &&
                        w.SubmittedAt.Date == DateTime.Now.Date)
                    .OrderByDescending(w => w.SubmittedAt)
                    .ToListAsync();
            }

            // Lưu tiến độ nếu đạt >= 80%
            if (score >= 80)
            {
                var quizCompletion = new QuizCompletion
                {
                    EnrollementId = enrollment.EnrollementId,
                    QuizId = model.QuizId,
                    Score = score,
                    CompletedAt = DateTime.Now
                };
                _dbContext.QuizCompletions.Add(quizCompletion);

                // Cập nhật tiến độ khóa học
                var courseProgress = await _dbContext.UserCourseProgresses
                    .FirstOrDefaultAsync(p => p.EnrollementId == enrollment.EnrollementId);

                if (courseProgress != null)
                {
                    courseProgress.CompletedQuizzes = await _dbContext.QuizCompletions
                        .CountAsync(qc => qc.EnrollementId == enrollment.EnrollementId);
                    courseProgress.ProgressPercentage = (decimal?)CalculateProgressPercentage(
                        courseProgress.CompletedLessons,
                        courseProgress.TotalLessons,
                        courseProgress.CompletedQuizzes,
                        courseProgress.TotalQuizzes);
                    courseProgress.UpdatedAt = DateTime.Now;
                    _dbContext.UserCourseProgresses.Update(courseProgress);
                }

                await _dbContext.SaveChangesAsync();
            }

            // Chuẩn bị view model cho kết quả
            var quizViewModel = new QuizViewModel
            {
                Quiz = quiz,
                CourseId = model.CourseId,
                LessonId = model.LessonId
            };

            var resultViewModel = new QuizResultViewModel
            {
                Score = score,
                CourseProgress = await _dbContext.UserCourseProgresses
                    .FirstOrDefaultAsync(p => p.EnrollementId == enrollment.EnrollementId),
                WritingSubmissions = writingSubmissions,
                 QuestionResults = questionResultsDict,  // Thêm kết quả đúng/sai của từng câu
                UserAnswers = userAnswersDict           // Thêm câu trả lời của người dùng
            };

            ViewBag.ShowQuiz = true;
            ViewBag.QuizViewModel = quizViewModel;
            ViewBag.QuizResult = resultViewModel;

            // Lấy thông tin khóa học để hiển thị danh sách lesson
            var courseDetailViewModel = await GetCourseDetailViewModel(model.CourseId, model.LessonId, userId);
            return View("CourseDetail", courseDetailViewModel);
        }
        // Phương thức GetCourseDetailViewModel
        private async Task<CourseDetailViewModel> GetCourseDetailViewModel(int courseId, int lessonId, int userId)
        {
            var course = await _dbContext.Courses
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
            {
                throw new Exception("Course not found");
            }

            var enrollment = await _dbContext.Enrollements
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == userId);

            if (enrollment == null)
            {
                throw new Exception("User not enrolled in this course");
            }

            var courseProgress = await _dbContext.UserCourseProgresses
                .FirstOrDefaultAsync(p => p.EnrollementId == enrollment.EnrollementId);

            if (courseProgress == null)
            {
                int totalLessons = course.Sections.Sum(s => s.Lessons.Count);
                int totalQuizzes = await _dbContext.LessonQuizMappings
                    .Where(lq => lq.Lesson.Section.CourseId == courseId)
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

            var completedLessons = await _dbContext.LessonCompletions
                .Where(lc => lc.EnrollementId == enrollment.EnrollementId)
                .Select(lc => lc.LessonId)
                .ToListAsync();

            var completedLessonsDict = completedLessons.ToDictionary(id => id, id => true);

            var completedQuizzes = await _dbContext.QuizCompletions
                .Where(qc => qc.EnrollementId == enrollment.EnrollementId)
                .Select(qc => qc.QuizId)
                .ToListAsync();

            var completedQuizzesDict = completedQuizzes.ToDictionary(id => id, id => true);

            var currentUser = await _dbContext.Users.FindAsync(userId);

            var comments = await _dbContext.QuizComments
                .Where(c => c.LessonId == lessonId && c.ParentCommentId == null)
                .Include(c => c.User)
                .Include(c => c.InverseParentComment)
                    .ThenInclude(r => r.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var allLessonIds = course.Sections.SelectMany(s => s.Lessons).Select(l => l.LessonId).ToList();
            var lessonQuizMappings = await _dbContext.LessonQuizMappings
                .Where(lq => allLessonIds.Contains(lq.LessonId))
                .Include(lq => lq.Quiz)
                .ToListAsync();

            var lessonQuizzes = new Dictionary<int, List<Quiz>>();
            foreach (var mapping in lessonQuizMappings)
            {
                if (!lessonQuizzes.ContainsKey(mapping.LessonId))
                {
                    lessonQuizzes[mapping.LessonId] = new List<Quiz>();
                }
                lessonQuizzes[mapping.LessonId].Add(mapping.Quiz);
            }

            return new CourseDetailViewModel
            {
                Course = course,
                UserId = userId,
                MentorId = course.CreatedBy,
                Enrollement = enrollment,
                CourseProgress = courseProgress,
                CompletedLessonIds = completedLessonsDict,
                CompletedQuizIds = completedQuizzesDict,
                LessonQuizzes = lessonQuizzes,
                Comments = comments,
                CurrentLessonId = lessonId,
                CurrentUserAvatar = currentUser?.AvatarUrl
            };
        }
        // Hàm hỗ trợ tính phần trăm tiến độ
        private double CalculateProgressPercentage(int? completedLessons, int totalLessons, int? completedQuizzes, int totalQuizzes)
        {
            // Gán giá trị mặc định là 0 nếu null
            int completedLessonsValue = completedLessons ?? 0;
            int completedQuizzesValue = completedQuizzes ?? 0;

            int totalItems = totalLessons + totalQuizzes;
            int completedItems = completedLessonsValue + completedQuizzesValue;

            return totalItems > 0 ? (double)completedItems / totalItems * 100 : 0;
        }





    }
}
