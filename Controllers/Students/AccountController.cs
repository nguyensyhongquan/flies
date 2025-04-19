using FliesProject.Data;
using FliesProject.Models.Entities;
using FliesProject.Repositories.IGenericRepository;
using FliesProject.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Shared;
using System.Security.Cryptography;
using System.Text;
using FliesProject.Services;
using Microsoft.AspNetCore.Authorization;
using FliesProject.Models.Entities.ViewModels;

namespace FliesProject.Controllers.Students
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly FiliesContext _dbContext;
        private readonly BlobStorageService _blobService;
        public AccountController(FiliesContext dbContext, IUserService userService, BlobStorageService blobService)
        {
            _userService = userService;
            _dbContext = dbContext;
            _blobService = blobService;
        }
        [AllowAnonymous]
        public IActionResult Home()
        {
            return View();
        }
        public IActionResult Dinosaur()
        {
            return View();
        }
        private User LoadUserWithEnrollmentsAndLatestCourse(out string latestCourseTitle)
        {
            latestCourseTitle = null;
            var username = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(username))
            {
                return null;
            }

            var user = _userService.GetUserByUsername(username);

            if (user == null)
            {
                return null;
            }

            _dbContext.Entry(user)
                      .Collection(u => u.EnrollementStudents)
                      .Query()
                      .Include(e => e.Course)
                      .Load();

            latestCourseTitle = user.EnrollementStudents
                .Where(e => e.Course != null)
                .OrderByDescending(e => e.StartedAt)
                .FirstOrDefault()?.Course?.Title;

            return user;
        }
        private IActionResult WithUserAndLatestCourse(Func<User, IActionResult> action)
        {
            var user = LoadUserWithEnrollmentsAndLatestCourse(out var latestCourseTitle);
            if (user == null)
            {
                return RedirectToAction("Home", "Account");
            }

            ViewBag.LatestCourseTitle = latestCourseTitle;
            return action(user);
        }
        public IActionResult ProfileStudent()
        {
            return WithUserAndLatestCourse(user => View(user));
        }

        public IActionResult TransactionHis(int page = 1)
        {
            const int PageSize = 5;

            var user = LoadUserWithEnrollmentsAndLatestCourse(out var latestCourseTitle);
            if (user == null)
            {
                return RedirectToAction("Home", "Account");
            }

            ViewBag.LatestCourseTitle = latestCourseTitle;
            var userId = user.UserId;

            var quizTransactions = _dbContext.QuizTransactions
                .Where(q => q.UserId == userId)
                .Select(q => new TransactionHistoryViewModel
                {
                    TransactionId = "QT" + q.QuiztransactionId,
                    TransactionDate = q.TransactionDate ?? DateTime.MinValue,
                    Amount = q.Amount,
                    Description = "Thanh toán bài quiz: " + q.Quiz.Title
                });

            var courseTransactions = _dbContext.CourseTransactions
                .Where(c => c.Enrollement.StudentId == userId)
                .Select(c => new TransactionHistoryViewModel
                {
                    TransactionId = "CT" + c.TransactionId,
                    TransactionDate = c.TransactionDate ?? DateTime.MinValue,
                    Amount = c.Amount,
                    Description = "Thanh toán khóa học: " + c.Enrollement.Course.Title
                });

            var allTransactions = quizTransactions
                .Union(courseTransactions)
                .OrderByDescending(t => t.TransactionDate)
                .ToList();

            // Total number of transactions
            var totalTransactions = allTransactions.Count();

            // Total number of pages
            var totalPages = (int)Math.Ceiling((double)totalTransactions / PageSize);

            // Paginate the transactions
            var paginated = allTransactions
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Create the view model to pass to the view
            var model = new StudentTransactionViewModel
            {
                User = user,
                Transactions = paginated,
                CurrentPage = page,
                TotalPages = totalPages,
                LatestCourseTitle = latestCourseTitle
            };

            // Return the view with the model
            return View(model);
        }
        public IActionResult ListCourse(int page = 1)
        {
            const int PageSize = 5;

            var user = LoadUserWithEnrollmentsAndLatestCourse(out var latestCourseTitle);
            if (user == null)
            {
                return RedirectToAction("Home", "Account");
            }

            ViewBag.LatestCourseTitle = latestCourseTitle;

            var enrolledCoursesQuery = _dbContext.Enrollements
                .Where(e => e.StudentId == user.UserId)
                .Select(e => new EnrolledCourseViewModel
                {
                    CourseTitle = e.Course.Title,
                    StartedAt = e.StartedAt,
                    EnrollmentId = e.EnrollementId,
                    ProgressPercentage = e.UserCourseProgresses
                        .OrderByDescending(p => p.UpdatedAt)
                        .Select(p => p.ProgressPercentage ?? 0)
                        .FirstOrDefault()
                })
                .OrderByDescending(e => e.StartedAt);

            var totalCourses = enrolledCoursesQuery.Count();
            var totalPages = (int)Math.Ceiling((double)totalCourses / PageSize);

            var paginatedCourses = enrolledCoursesQuery
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var model = new StudentCourseListViewModel
            {
                User = user,
                EnrolledCourses = paginatedCourses,
                CurrentPage = page,
                TotalPages = totalPages,
                LatestCourseTitle = latestCourseTitle
            };

            return View(model);
        }
        public IActionResult ListCertificate(int page = 1)
        {
            const int PageSize = 5;

            var user = LoadUserWithEnrollmentsAndLatestCourse(out var latestCourseTitle);
            if (user == null)
            {
                return RedirectToAction("Home", "Account");
            }

            var completedCourses = _dbContext.UserCourseProgresses
                .Where(p => p.ProgressPercentage == 100 && p.Enrollement.StudentId == user.UserId)
                .Select(p => new CertificateCourseViewModel
                {
                    CourseTitle = p.Enrollement.Course.Title,
                    StartedAt = p.Enrollement.StartedAt,
                    LimitedAt = p.Enrollement.LimitedAt,
                    ProgressPercentage = p.ProgressPercentage ?? 0,
                    EnrollementId = p.EnrollementId
                })
                .OrderByDescending(p => p.LimitedAt)
                .ToList();

            int totalCourses = completedCourses.Count();
            int totalPages = (int)Math.Ceiling((double)totalCourses / PageSize);
            var paginated = completedCourses
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var model = new CertificateListViewModel
            {
                User = user,
                Courses = paginated,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(model);
        }
        public IActionResult ViewCertificate(int enrollmentId)
        {
            // Lấy thông tin người dùng từ session
            var user = LoadUserWithEnrollmentsAndLatestCourse(out var latestCourseTitle);
            if (user == null)
            {
                return RedirectToAction("Home", "Account");
            }

            // Lấy thông tin khóa học hoàn thành
            var enrollment = _dbContext.UserCourseProgresses
                .Where(p => p.EnrollementId == enrollmentId && p.Enrollement.StudentId == user.UserId && p.ProgressPercentage == 100)
                .FirstOrDefault();

            if (enrollment == null)
            {
                // In ra console để kiểm tra lý do không tìm thấy
                Console.WriteLine($"Enrollment not found for StudentId: {user.UserId}, EnrollmentId: {enrollmentId}");
                return RedirectToAction("ListCertificate", "Account");
            }

            // Tạo ViewModel để truyền vào View
            var certificateViewModel = new CertificateViewModel
            {
                StudentName = user.Fullname,
                CourseTitle = enrollment.Enrollement.Course.Title,
                IssueDate = DateTime.Now
            };

            return View(certificateViewModel);
        }
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserName");

            return RedirectToAction("Home", "Account");
        }


        //Register code 
        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists
                var existingUsername = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
                if (existingUsername != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }

                // Check if email already exists
                var existingEmail = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng");
                    return View(model);
                }

                // Create new user
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    Passwordhash = HashPassword(model.Password),
                    Role = "student", // Default role is student
                    Status = "active",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    // Explicitly set nullable strings to empty to avoid potential UNIQUE NULL issues
                    Fullname = "", // Or null, depending on desired default
                    AvatarUrl = "", // Or null
                    Address = "",   // Or null
                    PhoneNumber = "" // Set to empty string instead of null
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Home", "Account");
            }

            return View(model);
        }

        // Helper method to hash password
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        [HttpPost]
        public IActionResult UpdateProfile(string fullName, string birthday, string gender, string phone, string address)
        {
            var username = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(username))
            {
                return Json(new { success = false, message = "User not logged in" });
            }

            var user = _userService.GetUserByUsername(username);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Parse birthday (DateTime)
            DateTime parsedBirthday;
            if (!DateTime.TryParseExact(birthday, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out parsedBirthday))
            {
                return Json(new { success = false, message = "Invalid date format" });
            }

            // Cập nhật thông tin
            user.Fullname = fullName;
            user.Birthday = parsedBirthday;
            user.Gender = gender;
            user.PhoneNumber = phone;
            user.Address = address;

            try
            {
                _dbContext.Users.Update(user);
                _dbContext.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Update failed: " + ex.Message });
            }
        }
        [HttpPost]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var username = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(username))
            {
                return Json(new { success = false, message = "User not logged in" });
            }

            var user = _userService.GetUserByUsername(username);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            if (!VerifyPassword(user.Passwordhash, currentPassword))
            {
                return Json(new { success = false, message = "Mật khẩu cũ không đúng" });
            }

            if (newPassword != confirmPassword)
            {
                return Json(new { success = false, message = "Mật khẩu mới và mật khẩu xác nhận không khớp" });
            }

            if (newPassword.Length < 6)
            {
                return Json(new { success = false, message = "Mật khẩu mới phải có ít nhất 6 ký tự" });
            }

            // Mã hóa mật khẩu mới và lưu vào cơ sở dữ liệu
            user.Passwordhash = HashPassword(newPassword);

            try
            {
                _dbContext.Users.Update(user);
                _dbContext.SaveChanges();

                return Json(new { success = true, message = "Đổi mật khẩu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đổi mật khẩu thất bại: " + ex.Message });
            }
        }
        private bool VerifyPassword(string hashedPassword, string plainPassword)
        {
            var computedHash = HashPassword(plainPassword);
            return computedHash == hashedPassword;
        }
        [HttpPost]
        public async Task<IActionResult> ChangeAvatar(IFormFile avatar)
        {
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(username) || avatar == null || avatar.Length == 0)
                return Json(new { success = false, message = "Vui lòng chọn file ảnh hợp lệ." });

            var user = _userService.GetUserByUsername(username);
            if (user == null)
                return Json(new { success = false, message = "Người dùng không tồn tại." });

            try
            {
                // 1. Tạo tên file duy nhất
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(avatar.FileName)}";

                // 2. Upload lên blob và nhận URL
                string blobUrl;
                using (var stream = avatar.OpenReadStream())
                {
                    blobUrl = await _blobService.UploadFileAsync(stream, fileName);
                }

                // 3. Cập nhật URL vào database
                user.AvatarUrl = blobUrl;
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();

                return Json(new { success = true, newAvatarUrl = blobUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Upload thất bại: " + ex.Message });
            }
        }
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return Content("Không có token.");
            }

            try
            {
                var decodedBytes = Convert.FromBase64String(token);
                var raw = Encoding.UTF8.GetString(decodedBytes);

                var parts = raw.Split(':');
                if (parts.Length != 2)
                {
                    return Content("Liên kết đặt lại mật khẩu không hợp lệ.");
                }

                var userId = parts[0];
                var ticks = long.Parse(parts[1]);
                var tokenTime = new DateTime(ticks, DateTimeKind.Utc);
                var now = DateTime.UtcNow;

                // Kiểm tra hiệu lực 5 phút
                if ((now - tokenTime).TotalMinutes > 5)
                {
                    return Content("Liên kết đã hết hạn sau 5 phút.");
                }

                ViewBag.UserId = userId;
                return View();
            }
            catch
            {
                return Content("Liên kết đặt lại mật khẩu không hợp lệ.");
            }
        }
        [HttpPost]
        public IActionResult UpdatePasswordFromReset([FromBody] ResetPasswordModel model)
        {
            try
            {
                var user = _userService.GetUserById(model.UserId);
                if (user == null)
                {
                    return Json(new { success = false, message = "Người dùng không tồn tại." });
                }

                // Hash the new password before saving it
                var hashedPassword = HashPassword(model.NewPassword);
                _userService.UpdatePassword(model.UserId, hashedPassword);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }


    }
}


