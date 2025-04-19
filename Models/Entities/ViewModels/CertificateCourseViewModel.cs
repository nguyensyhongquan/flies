namespace FliesProject.Models.Entities.ViewModels
{
    public class CertificateCourseViewModel
    {
        public string CourseTitle { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? LimitedAt { get; set; }
        public decimal ProgressPercentage { get; set; }
        public int EnrollementId { get; set; }
    }
}
