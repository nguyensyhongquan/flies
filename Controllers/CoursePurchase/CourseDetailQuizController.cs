using FliesProject.Data;
using FliesProject.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FliesProject.Controllers.CoursePurchase
{
    public class CourseDetailQuizController : Controller
    {
        private readonly FiliesContext _dbContext;

        public CourseDetailQuizController(FiliesContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetQuizInfo(int id, int lessonId)
        {
            try
            {
                // Lấy thông tin người dùng hiện tại
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // Kiểm tra xem quiz có thuộc lesson không qua LessonQuizMapping
                var quizMapping = await _dbContext.LessonQuizMappings
                    .Where(lq => lq.QuizId == id && lq.LessonId == lessonId)
                    .Include(lq => lq.Quiz)
                        .ThenInclude(q => q.QuizQuestions)
                            .ThenInclude(qq => qq.QuizAnswers)
                    .Include(lq => lq.Lesson)
                        .ThenInclude(l => l.Section)
                            .ThenInclude(s => s.Course)
                    .FirstOrDefaultAsync();

                if (quizMapping == null)
                {
                    Console.WriteLine($"QuizId {id} not found for LessonId {lessonId} in LessonQuizMapping");
                    return NotFound(new { message = $"Quiz with ID {id} not found for Lesson ID {lessonId}." });
                }

                var quiz = quizMapping.Quiz;
                var courseId = quizMapping.Lesson.Section.CourseId;

                // Kiểm tra xem người dùng có ghi danh vào khóa học không
                var enrollment = await _dbContext.Enrollements
                    .FirstOrDefaultAsync(e => e.StudentId == userId && e.CourseId == courseId);

                if (enrollment == null)
                {
                    Console.WriteLine($"User {userId} not enrolled in CourseId {courseId}");
                    return Forbid();
                }

                // Đảm bảo QuizQuestions không null
                if (quiz.QuizQuestions == null)
                {
                    quiz.QuizQuestions = new List<QuizQuestion>();
                }

                // Kiểm tra mối quan hệ Quiz.Courses (nếu cần)
                if (quiz.Courses.Any() && !quiz.Courses.Any(c => c.CourseId == courseId))
                {
                    Console.WriteLine($"QuizId {id} not associated with CourseId {courseId}");
                    return BadRequest(new { message = $"Quiz is not associated with the course." });
                }

                Console.WriteLine($"Quiz found: {quiz.Title} for LessonId {lessonId}, CourseId {courseId}");
                return Json(new
                {
                    quiz.QuizId,
                    quiz.Title,
                    quiz.QuizType,
                    quiz.Content,
                    quiz.MediaUrl,
                    quiz.TimeLimit,
                    QuizQuestions = quiz.QuizQuestions.Select(qq => new
                    {
                        qq.QuestionId,
                        qq.QuestionText,
                        qq.QuestionType,
                        qq.MediaUrl,
                        QuizAnswers = qq.QuizAnswers.Select(qa => new
                        {
                            qa.AnswerId,
                            qa.AnswerText,
                            qa.IsCorrect
                        })
                    })
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetQuizInfo: QuizId {id}, LessonId {lessonId}, Error: {ex.Message}");
                return StatusCode(500, new { message = "Server error while retrieving quiz." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitQuiz([FromBody] QuizSubmissionModel model)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier));

                // Step 1: Get the CourseId from the LessonId
                var courseId = await _dbContext.Lessons
                    .Where(l => l.LessonId == model.LessonId)
                    .Select(l => l.Section.CourseId)
                    .FirstOrDefaultAsync();

                if (courseId == 0) // Handle case where LessonId is invalid
                {
                    return NotFound("Lesson not found.");
                }

                // Step 2: Get the enrollment using the CourseId
                var enrollment = await _dbContext.Enrollements
                    .FirstOrDefaultAsync(e => e.StudentId == userId && e.CourseId == courseId);

                if (enrollment == null)
                {
                    return Forbid();
                }

                // Update or create quiz completion
                var quizCompletion = await _dbContext.QuizCompletions
                    .FirstOrDefaultAsync(qc => qc.EnrollementId == enrollment.EnrollementId && qc.QuizId == model.QuizId);

                if (quizCompletion == null)
                {
                    quizCompletion = new QuizCompletion
                    {
                        EnrollementId = enrollment.EnrollementId,
                        QuizId = model.QuizId,
                        Score = (int)model.Score,
                        CompletedAt = DateTime.Now
                    };
                    _dbContext.QuizCompletions.Add(quizCompletion);
                }
                else
                {
                    quizCompletion.Score = (int)model.Score;
                    quizCompletion.CompletedAt = DateTime.Now;
                }

                // Update course progress
                var courseProgress = await _dbContext.UserCourseProgresses
                    .FirstOrDefaultAsync(p => p.EnrollementId == enrollment.EnrollementId);

                if (courseProgress != null && model.IsCompleted)
                {
                    if (!await _dbContext.QuizCompletions.AnyAsync(qc => qc.EnrollementId == enrollment.EnrollementId && qc.QuizId == model.QuizId && qc.Score >= 80))
                    {
                        courseProgress.CompletedQuizzes++;
                        courseProgress.ProgressPercentage = (decimal)(((float)(courseProgress.CompletedLessons + courseProgress.CompletedQuizzes) / (courseProgress.TotalLessons + courseProgress.TotalQuizzes)) * 100);
                        courseProgress.UpdatedAt = DateTime.Now;
                    }
                }

                await _dbContext.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi lưu kết quả quiz." });
            }
        }


    }

    public class QuizSubmissionModel
    {
        public int QuizId { get; set; }
        public int LessonId { get; set; }
        public double Score { get; set; }
        public bool IsCompleted { get; set; }
    }
}



