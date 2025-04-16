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
}
