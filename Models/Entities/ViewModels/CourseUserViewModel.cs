namespace FliesProject.Models.Entities.ViewModels
{
    public class CourseUserViewModel
    {
        public List<Course> Courses { get; set; }
        public User User { get; set; }
    }

    public class UserViewModel
    {
        public string Fullname { get; set; }
        public string Role { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class CourseManageViewModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public List<SectionViewModel> Sections { get; set; } = new List<SectionViewModel>();
    }

    public class SectionViewModel
    {
        public int SectionId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public List<LessonViewModel> Lessons { get; set; } = new List<LessonViewModel>();
    }

    public class LessonViewModel
    {
        public int LessonId { get; set; }
        public string Title { get; set; } = null!;
        public string LessonType { get; set; } = null!; // "Content" or "Quiz"
        public string? VideoUrl { get; set; } // URL for content or placeholder for quiz
        public List<int>? QuizIds { get; set; } // For quiz-based lessons
        public List<string>? QuizTitles { get; set; } // For display
        public int? Duration { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
    public class CourseWithEnrollmentViewModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string? CoursesPicture { get; set; }
        public int EnrollmentCount { get; set; }
    }
    public class AddSectionViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class AddLessonViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = null!;
        public int SectionId { get; set; }
        public string Title { get; set; } = null!;
        public string LessonType { get; set; } = null!; // "Content" or "Quiz"
        public int[] QuizIds { get; set; } = Array.Empty<int>(); // For quiz-based lessons
        public int? Duration { get; set; }
        public List<SectionViewModel> Sections { get; set; } = new List<SectionViewModel>();
        public List<QuizViewModel> Quizzes { get; set; } = new List<QuizViewModel>();
    }
    public class QuizViewModel
    {
        public int QuizId { get; set; }
        public string Title { get; set; }
    }
    public class QuizDetailsViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<QuizQuestionViewModel> Questions { get; set; }
    }

    public class QuizQuestionViewModel
    {
        public string Text { get; set; }
        public string Type { get; set; }
        public string MediaUrl { get; set; }
        public List<QuizAnswerViewModel> Answers { get; set; }
        public List<QuizWritingSampleViewModel> WritingSamples { get; set; }
    }

    public class QuizAnswerViewModel
    {
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class QuizWritingSampleViewModel
    {
        public string Sample { get; set; }
    }
}

