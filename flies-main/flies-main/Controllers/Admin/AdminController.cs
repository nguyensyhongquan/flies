using FliesProject.Repositories.IGenericRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FliesProject.Models.Entities;
using FliesProject.Extensions;
using FliesProject.Repositories.GenericRepository;
using Microsoft.AspNetCore.Authorization;
namespace FliesProject.Controllers.Admin
{
    [Authorize(Policy = "Admin")]
    public class AdminController : Controller
    {
        private readonly IStudentRepository _studentRepository;
       private readonly IUserRepository _userRepository;
        public AdminController(IStudentRepository studentRepository, IUserRepository userRepository)
        {
            _studentRepository = studentRepository;
            _userRepository = userRepository;
        }
        // GET: AdminController
       
        public ActionResult Home()
        {
            return View();
        }
        public ActionResult Test()
        {
            Console.WriteLine("Running to the ChatView ....................");
            return View();
        }
        public async Task<ActionResult> Student()
        {
            var StudentList = await _studentRepository.GetAllStudent();
            foreach(var student in StudentList)
            {
               Console.WriteLine(student.Fullname);
            }
            return View(StudentList);
        }
        public async Task<ActionResult> Mentor()
        {
            var MentorList = await _userRepository.GetAllUsersAsync();
            foreach (var student in MentorList)
            {
                Console.WriteLine(student.Fullname);
            }
            return View(MentorList);
        }
        public ActionResult AddStudent()
        {
            //ViewData["ErrorMessage"] = null;

            return View("~/Views/Admin/AddStudent.cshtml");
        }
        public ActionResult AddMentor()
        {
            //ViewData["ErrorMessage"] = null;

            return View("~/Views/Admin/AddMentor.cshtml");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddMentor(string username, string fullname, string email, string mobilephone, string password, string gender, string balance, string birthday,string address)
        {
            try
            {
                Console.WriteLine($"Received data: Username={username}, Fullname={fullname}, Email={email}, Phone={mobilephone}, Gender={gender}, Balance={balance}, Birthday={birthday}");

                // Kiểm tra email
                if (await _userRepository.IsEmailExists(email))
                {
                    Console.WriteLine("lỗi email");
                    TempData.SetErrorMessage("Email");
                    return View("~/Views/Admin/AddStudent.cshtml");
                }

                // Kiểm tra username
                if (await _userRepository.IsUsernameExists(username))
                {
                    Console.WriteLine("lỗi username");
                    TempData.SetErrorMessage("Username đã tồn tại trong hệ thống!");
                    return View("~/Views/Admin/AddStudent.cshtml");
                }

                // Chuyển đổi ngày sinh
                if (!DateTime.TryParse(birthday, out DateTime parsedBirthday))
                {
                    Console.WriteLine("lỗi ngày sinh");
                    TempData.SetErrorMessage("Ngày sinh không hợp lý");
                    return View("~/Views/Admin/AddStudent.cshtml");
                }


                // Chuyển đổi số dư
                if (!decimal.TryParse(balance, out decimal parsedBalance))
                {
                    Console.WriteLine("lỗi số dư");
                    TempData.SetErrorMessage("Số dư  không hợp lý");
                    return View("~/Views/Admin/AddStudent.cshtml");
                }

                // TODO: Thêm logic tạo user và student ở đây
                // Sau khi tạo thành công, chuyển hướng về trang danh sách

                var newUser = new User
                {
                    Email = email,
                    // Ban đầu, Passwordhash chứa mật khẩu dạng plain text
                    Passwordhash = password,
                    Fullname = fullname,
                    AvatarUrl = "https://fliesenglish2025.blob.core.windows.net/avater/defaultavatar.jpg",
                    Role = "Mentor",
                    Balance = 0.00m,
                    Gender = gender,
                    Username = username,
                    Birthday = parsedBirthday,
                    Address = address,
                    PhoneNumber = mobilephone,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Status = "active"
                };
                var createdUser = await _userRepository.CreateUserAsync(newUser);
                Console.WriteLine($"User created with ID: {createdUser.UserId}");
                return RedirectToAction(nameof(Student));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                TempData.SetErrorMessage("Xảy ra cm gi loi roi Quan oi");
                return View("~/Views/Admin/AddStudent.cshtml");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddStudent(string username, string fullname, string email, string mobilephone, string password, string gender, string balance, string birthday)
        {
            try
            {
                Console.WriteLine($"Received data: Username={username}, Fullname={fullname}, Email={email}, Phone={mobilephone}, Gender={gender}, Balance={balance}, Birthday={birthday}");

                // Kiểm tra email
                if (await _userRepository.IsEmailExists(email))
                {
                    Console.WriteLine("lỗi email");
                    TempData.SetErrorMessage("Email");
                    return View("~/Views/Admin/AddStudent.cshtml");
                }

                // Kiểm tra username
                if(await _userRepository.IsUsernameExists(username))
                {
                    Console.WriteLine("lỗi username");
                    TempData.SetErrorMessage("Username đã tồn tại trong hệ thống!");
                    return View("~/Views/Admin/AddStudent.cshtml");
                }

                // Chuyển đổi ngày sinh
                if (!DateTime.TryParse(birthday, out DateTime parsedBirthday))
                {
                    Console.WriteLine("lỗi ngày sinh");
                    TempData.SetErrorMessage("Ngày sinh không hợp lý");
                    return View("~/Views/Admin/AddStudent.cshtml");
                }


                // Chuyển đổi số dư
                if (!decimal.TryParse(balance, out decimal parsedBalance))
                {
                    Console.WriteLine("lỗi số dư");
                    TempData.SetErrorMessage("Số dư  không hợp lý");
                    return View("~/Views/Admin/AddStudent.cshtml");
                }

                // TODO: Thêm logic tạo user và student ở đây
                // Sau khi tạo thành công, chuyển hướng về trang danh sách

                var newUser = new User
                {
                    Email = email,
                    // Ban đầu, Passwordhash chứa mật khẩu dạng plain text
                    Passwordhash = password,
                    Fullname = fullname,
                    AvatarUrl = "https://fliesenglish2025.blob.core.windows.net/avater/defaultavatar.jpg",
                    Role = "Student",
                    Balance = 0.00m,
                    Gender = gender,
                    Username = username,
                    Birthday =parsedBirthday,
                    Address="Viet Nam",
                    PhoneNumber = mobilephone,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Status = "active"
                };
                var createdUser = await _userRepository.CreateUserAsync(newUser);
                Console.WriteLine($"User created with ID: {createdUser.UserId}");
                return RedirectToAction(nameof(Student));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                TempData.SetErrorMessage("Xảy ra cm gi loi roi Quan oi");
                return View("~/Views/Admin/AddStudent.cshtml");
            }
        }

        private void RedirectToPageResult(string v, object value)
        {
            throw new NotImplementedException();
        }

        public ActionResult ChatAi(int id)
        {
            return View();
        }

        // GET: AdminController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: AdminController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AdminController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AdminController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AdminController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AdminController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AdminController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserName");

            return RedirectToAction("Home", "Account");
        }



     
    }
}
