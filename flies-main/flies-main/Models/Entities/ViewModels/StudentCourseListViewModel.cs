namespace FliesProject.Models.Entities.ViewModels
{
    public class StudentCourseListViewModel
    {
        public User User { get; set; }
        public List<EnrolledCourseViewModel> EnrolledCourses { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string LatestCourseTitle { get; set; }
    }
}
