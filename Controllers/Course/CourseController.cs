using FliesProject.Data;
using Microsoft.AspNetCore.Mvc;

namespace FliesProject.Controllers.Course
{
    public class CourseController : Controller

    {
        private readonly FiliesContext _dbContext;
        public CourseController(FiliesContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult CourseOverview()
        {
            var courses = _dbContext.Courses.ToList(); // Assuming you have DbContext injected
            return View(courses);
        }

        public IActionResult CourseDescribe(int id)
        {
            var course = _dbContext.Courses.Find(id);

            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }
        
       
       

        


    }
}
