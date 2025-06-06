using FliesProject.Data;
using FliesProject.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FliesProject.Repositories.GenericRepository;
using FliesProject.Repositories.IGenericRepository;
using System.Diagnostics;
using FliesProject.Services;
using FliesProject.Models.Entities;
using Org.BouncyCastle.Crypto.Generators;
using FliesProject.AIBot;
using Microsoft.AspNetCore.Authentication.Cookies;
using FliesProject.MiddleWare;
using FliesProject.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddScoped<IChatAnalyzerService, ChatAnalyzerService>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<IIntentClassificationService, IntentClassificationService>();
builder.Services.AddScoped<IChatModule, GeneralChatModule>();
builder.Services.AddScoped<IChatModule, DatabaseChatModule>();
builder.Services.AddScoped<Generator>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    // Lấy Gemin ApiKey từ cấu hình
    string apiKey = config["Gemini:ApiKey"];
    return new Generator(apiKey);
});
builder.Services.Configure<DatabaseChatModule>(builder.Configuration.GetSection("DatabaseChatModule"));
builder.Services.AddScoped<IChatModule, DatabaseChatModule>();
builder.Services.AddScoped<ChatRouterService>();
// Đăng ký BlobStorageService dưới dạng singleton hoặc scoped
builder.Services.AddSingleton<BlobStorageService>();
//builder.Services.AddScoped<IAIService, AIService>();
// 🔹 Cấu hình Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // ⏳ Thời gian session hết hạn
    options.Cookie.HttpOnly = true; // 🔐 Bảo mật cookie session
    options.Cookie.IsEssential = true;
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Account/Home";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });
builder.Services.AddHttpContextAccessor(); // Cần thiết để sử dụng HttpContext.Session
builder.Services.AddDbContext<FiliesContext>(options =>

    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
//builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddControllersWithViews().AddCookieTempDataProvider();
builder.Services.AddRazorPages();
builder.Services.AddScoped<UserService>();
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddSignalR(); // Thêm SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
          policy.RequireRole("admin"));
    options.AddPolicy("User", policy =>
      policy.RequireRole("user"));
    options.AddPolicy("Mentor", policy =>
      policy.RequireRole("mentor"));  // Quyền truy c

});



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
app.UseAuthentication();
app.UseMiddleware<CustomAuthMiddleware>(); // Đăng ký middleware
app.UseAuthorization();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Home}/{id?}");


app.UseCors("AllowAll");
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    

app.Run();


