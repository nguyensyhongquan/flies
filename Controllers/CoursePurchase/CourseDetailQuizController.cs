using FliesProject.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> GetQuizInfo(int id)
        {
            var quiz = await _dbContext.Quizzes
                .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.QuizAnswers)
                .FirstOrDefaultAsync(q => q.QuizId == id);

            if (quiz == null)
            {
                return NotFound();
            }

            return Json(quiz);
        }
    }
}
