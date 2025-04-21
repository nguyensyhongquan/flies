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
            public string Title { get; set; }
            public int EnrollmentCount { get; set; }
            public List<SectionViewModel> Sections { get; set; } = new List<SectionViewModel>();
        }

        public class SectionViewModel
        {
            public int SectionId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public int Position { get; set; }
            public List<LessonViewModel> Lessons { get; set; } = new List<LessonViewModel>();
        }

        public class LessonViewModel
        {
            public int LessonId { get; set; }
            public string Title { get; set; }
            public string VideoUrl { get; set; }
            public int? Duration { get; set; }
            public DateTime? CreatedAt { get; set; }
        }

        public class CourseWithEnrollmentViewModel
        {
            public int CourseId { get; set; }
            public string Title { get; set; }
            public string CoursesPicture { get; set; }
            public int EnrollmentCount { get; set; }
        }
    }

