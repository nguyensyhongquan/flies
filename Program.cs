using FliesProject.Data;
using FliesProject.Models.Entities;
using FliesProject.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FliesProject.Repositories.GenericRepository;
using FliesProject.Repositories.IGenericRepository;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình DbContext với chuỗi kết nối (đảm bảo cấu hình FliesProjectContext có trong appsettings.json)
builder.Services.AddDbContext<FiliesContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FliesProjectContext")));

// Đăng ký IUserRepository và UserRepository vào DI container (nếu chưa đăng ký)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();


//using (var scope = app.Services.CreateScope())
//{
//    // Resolve IUserRepository
//    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

//    // Tạo đối tượng User mới
//    var newUser = new User
//    {
//        Email = "quanvs2003@gmail.com",
//        // Ban đầu, Passwordhash chứa mật khẩu dạng plain text
//        Passwordhash = "quanchin123",
//        Fullname = "Nguyen Sy Hong Quan",
//        AvatarUrl = "https://fliesenglish2025.blob.core.windows.net/avater/pngwing.com.png",
//        Role = "student",
//        Balance = 0.00m,
//        Gender = "F",
//        Username = "quanquan2003",
//        Birthday = new DateTime(2003, 7, 10),
//        Address = "Dong Nai",
//        PhoneNumber = "0961459598",
//        CreatedAt = DateTime.Now,
//        UpdatedAt = DateTime.Now,
//        Status = "active"
//    };

//    // Gọi CreateUserAsync để thêm user, phương thức này sẽ tự hash mật khẩu bên trong
//    var createdUser = await userRepository.CreateUserAsync(newUser);
//    Console.WriteLine("dhwajbjjbjbjbjbjbjbjbjbjbjbjbjbb32111111111");

//    //Console.WriteLine($"User created with ID: {createdUser.UserId}");
//}

// Phần cấu hình HTTP request pipeline của ứng dụng web
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Student}/{id?}");

app.Run();
