using FliesProject.Data;
using FliesProject.Models.Entities;
using FliesProject.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Shared;
using System.Security.Claims;

namespace FliesProject.Controllers.Course
{
    public class UsersCourseController : Controller
    {
        private readonly FiliesContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsersCourseController(FiliesContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult CourseOverview()
        {
            var courses = _dbContext.Courses.ToList(); // Assuming you have DbContext injected
            return View(courses);
        }

        // Action hiển thị popup xác nhận mua khóa học
        public async Task<IActionResult> ConfirmPurchase(int id)
        {
            // Kiểm tra đăng nhập
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home", new { returnUrl = Url.Action("CourseDescribe", "UsersCourse", new { id }) });
            }

            // Lấy thông tin khóa học
            var course = await _dbContext.Courses
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            // Kiểm tra xem người dùng đã mua khóa học chưa
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isEnrolled = await _dbContext.Enrollements
                .AnyAsync(e => e.CourseId == id && e.StudentId == userId);

            if (isEnrolled)
            {
                TempData["Message"] = "Bạn đã đăng ký khóa học này rồi!";
                return RedirectToAction("CourseDescribe", new { id });
            }

            // Trả về partial view cho modal xác nhận
            return PartialView("_ConfirmPurchasePartial", course);
        }

        // Action xử lý mua khóa học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase(int id)
        {
            // Kiểm tra đăng nhập
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }

            // Lấy thông tin người dùng hiện tại
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _dbContext.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Lấy thông tin khóa học
            var course = await _dbContext.Courses
                .Include(c => c.CreatedByNavigation)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            // Kiểm tra số dư
            if (user.Balance < course.Price)
            {
                TempData["ErrorMessage"] = "Balance is not enough to handle this transaction";
                return RedirectToAction("CourseDescribe", new { id });
            }

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Tạo Enrollment mới
                    var enrollment = new Enrollement
                    {
                        CourseId = course.CourseId,
                        StudentId = userId,
                        MentorId = course.CreatedBy,
                        StartedAt = DateTime.Now,
                        LimitedAt = course.Timelimit.HasValue ? DateTime.Now.AddDays(course.Timelimit.Value) : null,
                        Status = "Active"
                    };

                    _dbContext.Enrollements.Add(enrollment);
                    await _dbContext.SaveChangesAsync();

                    // Tạo CourseTransaction
                    var courseTransaction = new CourseTransaction
                    {
                        EnrollementId = enrollment.EnrollementId,
                        Amount = course.Price ?? 0,
                        TransactionDate = DateTime.Now
                    };

                    _dbContext.CourseTransactions.Add(courseTransaction);

                    // Trừ tiền từ tài khoản người dùng
                    user.Balance -= course.Price;
                    _dbContext.Users.Update(user);

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Buy Course Successfull!";
                    return RedirectToAction("CourseDescribe", new { id });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi thanh toán: " + ex.Message;
                    return RedirectToAction("CourseDescribe", new { id });
                }
            }
        }

        // Các action khác giữ nguyên
        public async Task<IActionResult> CourseDescribe(int id)
        {
            var course = await _dbContext.Courses
                .Include(c => c.CreatedByNavigation)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            // Kiểm tra xem người dùng đã đăng ký khóa học chưa
            bool isEnrolled = false;
            if (User.Identity.IsAuthenticated)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                isEnrolled = await _dbContext.Enrollements
                    .AnyAsync(e => e.CourseId == id && e.StudentId == userId);
            }

            ViewBag.IsEnrolled = isEnrolled;

            return View(course);
        }

        [HttpGet]
        public async Task<IActionResult> CourseDetail(int id)
        {
            // Lấy thông tin người dùng hiện tại
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Lấy thông tin khóa học, section và lesson
            var course = await _dbContext.Courses
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            // Kiểm tra xem người dùng có được ghi danh vào khóa học không
            var enrollment = await _dbContext.Enrollements
                .FirstOrDefaultAsync(e => e.CourseId == id && e.StudentId == userId);

            if (enrollment == null)
            {
                return Forbid();
            }

            // Tạo view model để truyền cho view
            var viewModel = new CourseDetailViewModel
            {
                Course = course,
                UserId = userId,
                MentorId = course.CreatedBy,
                Enrollement = enrollment
            };

            // Trả về dữ liệu cho view
            return View(viewModel);
        }
    }
}
