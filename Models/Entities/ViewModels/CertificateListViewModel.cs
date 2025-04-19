namespace FliesProject.Models.Entities.ViewModels
{
    public class CertificateListViewModel
    {
        public User User { get; set; }
        public List<CertificateCourseViewModel> Courses { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public string LatestCourseTitle { get; set; }
    }
}
