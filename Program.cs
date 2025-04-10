using FliesProject.Data;
using FliesProject.Models.Entities;
using FliesProject.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using FliesProject.Service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IUserService, UserService>();
// 🔹 Cấu hình Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // ⏳ Thời gian session hết hạn
    options.Cookie.HttpOnly = true; // 🔐 Bảo mật cookie session
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor(); // Cần thiết để sử dụng HttpContext.Session
builder.Services.AddDbContext<FiliesContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<UserService>();
builder.Services.AddControllersWithViews();
builder.Services.AddControllers().AddNewtonsoftJson();

var app = builder.Build();

// 🔹 Cấu hình Middleware: Phải gọi `UseSession()` trước `UseAuthorization()`
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Home}/{id?}");
app.Run();
