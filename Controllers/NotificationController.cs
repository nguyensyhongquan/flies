using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using FliesProject.Hubs;
using FliesProject.Models.Entities;
using FliesProject.Repositories;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using FliesProject.Repositories.IGenericRepository;

namespace FliesProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;

        public NotificationController(
            IHubContext<NotificationHub> hubContext,
            INotificationRepository notificationRepository,
            IUserRepository userRepository)
        {
            _hubContext = hubContext;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
        }

        // Lưu thông báo và gửi đến user
        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
        {
            try
            {
                // Validate request
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState.Values.SelectMany(v => v.Errors) });
                }

                // Lấy thông tin người gửi từ token
                var senderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(senderId))
                {
                    return Unauthorized(new { success = false, message = "Không tìm thấy thông tin người dùng" });
                }

                // Tạo thông báo mới
                var notification = new Notification
                {
                    Title = request.Title,
                    Message = request.Message,
                    NotificationType = request.Type,
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    Status = "Active",
                    IsUrgent = request.IsUrgent,
                    Link = request.Link,
                    SenderId = int.Parse(senderId),
                    UserId = 1 // Mặc định gửi cho admin
                };

                // Lưu vào database
                await _notificationRepository.AddAsync(notification);
                await _notificationRepository.SaveAsync();

                // Gửi thông báo real-time đến admin
                await _hubContext.Clients.User("1").SendAsync("ReceiveNotification", notification);

                return Ok(new { success = true, notification });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi gửi thông báo", error = ex.Message });
            }
        }

        // Đánh dấu thông báo đã đọc
        [HttpPost("mark-as-read/{notificationId}")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var notification = await _notificationRepository.GetByIdAsync(notificationId);

                if (notification == null)
                {
                    return NotFound(new { success = false, message = "Thông báo không tồn tại" });
                }

                if (notification.UserId != userId)
                {
                  
                }

                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
                await _notificationRepository.UpdateAsync(notification);
                await _notificationRepository.SaveAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi đánh dấu thông báo", error = ex.Message });
            }
        }

        // Lấy số thông báo chưa đọc
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var count = await _notificationRepository.GetUnreadCountAsync(userId);

                return Ok(new { success = true, count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi lấy số thông báo chưa đọc", error = ex.Message });
            }
        }

        // Lấy danh sách thông báo
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, page, pageSize);

                return Ok(new { success = true, notifications });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi lấy danh sách thông báo", error = ex.Message });
            }
        }

        // Đánh dấu tất cả thông báo đã đọc
        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                await _notificationRepository.MarkAllAsReadAsync(userId);
                await _notificationRepository.SaveAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi đánh dấu tất cả thông báo", error = ex.Message });
            }
        }
    }

    public class NotificationRequest
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(100, ErrorMessage = "Tiêu đề không được quá 100 ký tự")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        [StringLength(500, ErrorMessage = "Nội dung không được quá 500 ký tự")]
        public string Message { get; set; }

        [Required(ErrorMessage = "Loại thông báo không được để trống")]
        [StringLength(20, ErrorMessage = "Loại thông báo không hợp lệ")]
        public string Type { get; set; }

        public bool IsUrgent { get; set; }

        public string? Link { get; set; }
    }
} 