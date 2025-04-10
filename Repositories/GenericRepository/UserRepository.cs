using System.Diagnostics;
using FliesProject.Data;
using FliesProject.Models.Entities;
using FliesProject.Repositories.IGenericRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FliesProject.Repositories.GenericRepository
{
    public class UserRepository : IUserRepository
    {
        private readonly FiliesContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserRepository(FiliesContext context)
        {
            _context = context;
           
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<User> CreateUserAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(user.Passwordhash))
                throw new ArgumentException("Password cannot be null or empty");

            // Hash mật khẩu (Passwordhash ban đầu chứa plain text)
            user.Passwordhash = _passwordHasher.HashPassword(user, user.Passwordhash);

            _context.Users.Add(user);

            // Lưu thay đổi và lấy số dòng được ảnh hưởng
           // var rowsAffected = await _context.SaveChangesAsync();
           // Debug.WriteLine($"Rows affected: {rowsAffected}");
           //Debug.WriteLine("dhwajbjjbjbjbjbjbjbjbjbjbjbjbjbb");

            return user;
        }

        /// <summary>
        /// Lấy danh sách tất cả người dùng.
        /// </summary>
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        /// <summary>
        /// Lấy thông tin người dùng theo Id.
        /// </summary>
        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        /// <summary>
        /// Lấy thông tin người dùng theo email.
        /// </summary>
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        /// <summary>
        /// Cập nhật thông tin người dùng.
        /// Lưu ý: Nếu cập nhật mật khẩu, cần đảm bảo hash mật khẩu mới nếu nó đang ở dạng plain text.
        /// </summary>
        public async Task UpdateUserAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // Nếu bạn cần cập nhật mật khẩu, hãy kiểm tra và hash lại mật khẩu mới (nếu chưa được hash)
            // Ví dụ: kiểm tra một flag hay so sánh với chuỗi hash hiện tại
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Xóa người dùng theo Id.
        /// </summary>
        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}
