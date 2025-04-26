using FliesProject.Models.Entities;

namespace FliesProject.ViewModel
{
    public class QuizViewModel
    {
        public Quiz Quiz { get; set; }
        public int CourseId { get; set; }
        public int LessonId { get; set; }
    }

    public class QuizResultViewModel
    {
        public int Score { get; set; }
        public UserCourseProgress CourseProgress { get; set; }
    }
}
