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
    }
}
