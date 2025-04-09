using FliesProject.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<FiliesContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
//using FliesProject.Models.AImodel;

//public class Program()
//{
//    public static void Main(string[] args)
//    {

//        // Create a new Conversation instance
//        var conversation = new Conversation
//        {
//            Question = "Động từ đi sau recommend thì nên ở thể nào vậy?",
//            ImagesAsBase64 = new List<string>()
//        };

//        // Add History objects to the ChatHistory list
//        conversation.ChatHistory.Add(new Conversation.History
//        {
//            FromUser = true,
//            Message = "Hi i want to ask t"
//        });

//        conversation.ChatHistory.Add(new Conversation.History
//        {
//            FromUser = false,
//            Message = "I'm good, thank you! How can I assist you today?"
//        });

//        // Output the conversation details
//        foreach (var history in conversation.ChatHistory)
//        {
//            System.Console.WriteLine($"FromUser: {history.FromUser}, Message: {history.Message}");
//        }
//    }

//}
