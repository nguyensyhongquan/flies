using FliesProject.Models.Entities;

namespace FliesProject.ViewModel
{
    public class CourseDetailViewModel
    {
        public Course Course { get; set; }
        public int UserId { get; set; }
        public int MentorId { get; set; }
        public Enrollement Enrollement { get; set; }
        public UserCourseProgress CourseProgress { get; set; }
        public Dictionary<int, bool> CompletedLessonIds { get; set; } = new Dictionary<int, bool>();

        public Dictionary<int, bool> CompletedQuizIds { get; set; } = new Dictionary<int, bool>();

        public Dictionary<int, List<Quiz>> LessonQuizzes { get; set; } = new Dictionary<int, List<Quiz>>();

        // Comments related properties
        public List<QuizComment> Comments { get; set; } = new List<QuizComment>();
        public string NewCommentText { get; set; }
        public int CurrentLessonId { get; set; }
        public string CurrentUserAvatar { get; set; } // Add this property
                                                      
    }
}
