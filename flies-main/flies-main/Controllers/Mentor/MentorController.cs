using FliesProject.Data;
using FliesProject.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FliesProject.Controllers.Mentor;

public class MentorController : Controller
{
    private readonly FiliesContext _context;

    public MentorController(FiliesContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View(_context.Users.Where(r => r.Role.Equals("mentor")).ToList());
    }

    public IActionResult Student(int mentorId)
    {
        var students = _context.Enrollements
            .Include(e => e.Student)
            .Where(e => e.MentorId == mentorId && e.Student.Role.Equals("student"))
            .Select(e => e.Student)
            .ToList();

        return View(students);
    }
    public IActionResult StudentDetail(int studentId)
    {
        var students = _context.Enrollements
            .Include(e => e.Student)
            .Include(c => c.Course)
                .ThenInclude(c => c.Quizzes)
                    .ThenInclude(q => q.QuizQuestions) 
                        .ThenInclude(a => a.QuizAnswers)
            .Include(c => c.UserCourseProgresses)
            .FirstOrDefault(e => e.StudentId == studentId);
        var complete = students.UserCourseProgresses.First().CompletedQuizzes;
        var total = students.UserCourseProgresses.First().TotalQuizzes;
        ViewBag.Total = (double)complete / total * 100;
        return View(students);
    }
    public IActionResult Quesion(int studentId)
    {
        var question = _context.Enrollements
            .Include(e => e.Student)
             .Include(c => c.Course)
                .ThenInclude(c => c.Quizzes)
                    .ThenInclude(q => q.QuizQuestions)
                        .ThenInclude(a => a.QuizAnswers)
            .FirstOrDefault(e => e.StudentId == studentId && e.Student.Role.Equals("student"));
        return View(question);
    }
}