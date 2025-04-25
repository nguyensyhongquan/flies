//using FliesProject.Data;
//using FliesProject.Models;
//using FliesProject.Repositories.GenericRepository;
//using FliesProject.Repositories.IGenericRepository;
//using Microsoft.EntityFrameworkCore;
//using System.Linq.Expressions;
//using FliesProject.Models.Entities;
//namespace FliesProject.Repositories
//{
//    public interface INotificationRepository : IGenericRepository<Notification>
//    {
//        Task<int> GetUnreadCountAsync(int userId);
//        Task<List<Notification>> GetUserNotificationsAsync(int userId, int page, int pageSize);
//        Task MarkAllAsReadAsync(int userId);
//        Task AddAsync(Notification notification);
//        Task UpdateAsync(Notification notification);
//        Task SaveAsync();
//        Task<Notification> GetByIdAsync(int notificationId);
//    }

//    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
//    {
//        private readonly FiliesContext _context;

//        public NotificationRepository(FiliesContext context) : base(context)
//        {
//            _context = context;
//        }

//        public async Task<int> GetUnreadCountAsync(int userId)
//        {
//            return await _context.Notifications
//                .CountAsync(n => n.UserId == userId && n.IsRead == false);
//        }

//        public async Task<List<Notification>> GetUserNotificationsAsync(int userId, int page, int pageSize)
//        {
//            return await _context.Notifications
//                .Where(n => n.UserId == userId)
//                .OrderByDescending(n => n.NotificationId)
//                .Skip((page - 1) * pageSize)
//                .Take(pageSize)
//                .ToListAsync();
//        }

//        public async Task MarkAllAsReadAsync(int userId)
//        {
//            var notifications = await _context.Notifications
//                .Where(n => n.UserId == userId && n.IsRead == false)
//                .ToListAsync();

//            foreach (var notification in notifications)
//            {
//                notification.IsRead = true;
//                notification.ReadAt = DateTime.Now;
//            }

//            await _context.SaveChangesAsync();
//        }

//        public async Task AddAsync(Notification notification)
//        {
//            await _context.Notifications.AddAsync(notification);
//        }

//        public async Task UpdateAsync(Notification notification)
//        {
//            _context.Notifications.Update(notification);
//        }

//        public async Task SaveAsync()
//        {
//            await _context.SaveChangesAsync();
//        }

//        public async Task<Notification> GetByIdAsync(int notificationId)
//        {
//            return await _context.Notifications
//                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);
//        }
//    }
//} 