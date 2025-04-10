using FliesProject.Data;
using FliesProject.Models.Entities;
using FliesProject.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FliesProject.Repositories.GenericRepository;
using FliesProject.Repositories.IGenericRepository;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<FliesProject.Data.FiliesContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Đăng ký IUserRepository và UserRepository vào DI container (nếu chưa đăng ký)
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Code thử nghiệm thêm user trong một scope
using (var scope = app.Services.CreateScope())
{
    // Resolve IUserRepository
    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

    // Tạo đối tượng User mới
    var newUser = new User
    {
        Email = "nguyensyhongquan130703@gmail.com",
        // Ban đầu, Passwordhash chứa mật khẩu dạng plain text
        Passwordhash = "quanchin123",
        Fullname = "Nguyen Sy Hong Quan",
        AvatarUrl = "https://fliesenglish2025.blob.core.windows.net/avater/pngwing.com.png",
        Role = "admin",
        Balance = 0.00m,
        Gender = "M",
        Username = "quanvs2003",
        Birthday = new DateTime(2003, 7, 13),
        Address = "Nghe An",
        PhoneNumber = "0343413939",
        CreatedAt = DateTime.Now,
        UpdatedAt = DateTime.Now,
        Status = "active"
    };

    // Gọi CreateUserAsync để thêm user, phương thức này sẽ tự hash mật khẩu bên trong
    var createdUser = await userRepository.CreateUserAsync(newUser);
    Console.WriteLine("dhwajbjjbjbjbjbjbjbjbjbjbjbjbjbb32111111111");

    Console.WriteLine($"User created with ID: {createdUser.UserId}");
}

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
