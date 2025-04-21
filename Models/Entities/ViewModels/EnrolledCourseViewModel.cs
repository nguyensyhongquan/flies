namespace FliesProject.Models.Entities.ViewModels
{
    public class EnrolledCourseViewModel
    {
        public int EnrollmentId { get; set; }
        public string CourseTitle { get; set; }
        public DateTime? StartedAt { get; set; }
        public decimal ProgressPercentage { get; set; }
    }
}
