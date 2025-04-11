using FliesProject.Data;
using FliesProject.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FliesProject.Repositories.GenericRepository;
using FliesProject.Repositories.IGenericRepository;
using System.Diagnostics;
using FliesProject.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddScoped<IChatAnalyzerService, ChatAnalyzerService>();
//builder.Services.AddScoped<IAIService, AIService>();
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

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddControllersWithViews().AddCookieTempDataProvider();
builder.Services.AddRazorPages();
builder.Services.AddScoped<UserService>();
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
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",




    pattern: "{controller=Account}/{action=Home}/{id?}");

app.Run();
