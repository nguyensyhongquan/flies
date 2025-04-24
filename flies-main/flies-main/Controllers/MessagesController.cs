using FliesProject.Data;
using FliesProject.Models.Entities;
using FliesProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FliesProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly FiliesContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessagesController(FiliesContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        public IActionResult GetMessages([FromQuery] int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (id <= 0)
            {
                return BadRequest("ID không hợp lệ");
            }

            IQueryable<Message> query = _context.Messages
                .Include(m => m.Student)
                .Include(m => m.Mentor);

            if (userRole == "mentor")
            {
                query = query.Where(m => m.MentorId == userId && m.StudentId == id);
            }
            else if (userRole == "student")
            {
                query = query.Where(m => m.StudentId == userId && m.MentorId == id);
            }
            else
            {
                return Unauthorized("Vai trò không hợp lệ");
            }

            var messages = query
                .OrderBy(m => m.Time)
                .Select(m => new
                {
                    StudentName = m.Student != null ? m.Student.Fullname : "Không xác định",
                    MentorName = m.Mentor != null ? m.Mentor.Fullname : "Không xác định",
                    m.Sender,
                    m.Content,
                    m.Time
                })
                .ToList();

            return Ok(messages);
        }


        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] MessageDto messageDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null || messageDto.StudentId <= 0 || string.IsNullOrEmpty(messageDto.Content))
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            var message = new Message
            {
                StudentId = userRole == "student" ? userId : messageDto.StudentId,
                MentorId = userRole == "mentor" ? userId : messageDto.StudentId,
                Sender = userRole == "mentor" ? "mentor" : user.Fullname,
                Content = messageDto.Content,
                Time = DateTime.Now.ToString("HH:mm")
            };

            if (message.MentorId <= 0 || message.StudentId <= 0)
            {
                return BadRequest("Không tìm thấy Mentor hoặc Student cho cuộc trò chuyện");
            }

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Gửi tin nhắn qua SignalR
            await _hubContext.Clients.Group($"Chat_{message.StudentId}_{message.MentorId}")
                .SendAsync("ReceiveMessage", message.StudentId, message.MentorId, message.Sender, message.Content, message.Time);

            return Ok();
        }

        [HttpGet("students")]
        public IActionResult GetStudents()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole == "mentor")
            {
                var students = _context.Enrollements
                    .Where(e => e.MentorId == userId)
                    .Select(e => new { Id = e.StudentId, Name = e.Student.Fullname })
                    .Distinct()
                    .ToList();
                return Ok(students);
            }
            else if (userRole == "student")
            {
                var mentors = _context.Enrollements
                    .Where(e => e.StudentId == userId)
                    .Select(e => new { Id = e.MentorId, Name = e.Mentor.Fullname })
                    .Distinct()
                    .ToList();
                return Ok(mentors);
            }

            return Unauthorized("Vai trò không hợp lệ");
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _context.Users.FindAsync(userId);
            return Ok(new { Id = user.UserId, Fullname = user.Fullname, Role = User.FindFirst(ClaimTypes.Role)?.Value });
        }
    }

    public class MessageDto
    {
        public int StudentId { get; set; }
        public string Content { get; set; }
    }
}