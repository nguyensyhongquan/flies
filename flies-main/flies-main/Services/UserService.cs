using FliesProject.Data;
using FliesProject.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FliesProject.Services
{
    public class UserService : IUserService
    {
        private readonly FiliesContext _context;

        public UserService(FiliesContext context)
        {
            _context = context;
        }

        public User GetUserByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username);
        }
        public User GetUserByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
        }
        public User GetUserById(string userId)
        {
            return _context.Users.FirstOrDefault(u => u.UserId.ToString() == userId);
        }

        public void UpdatePassword(string userId, string newPassword)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId.ToString() == userId);
            if (user != null)
            {
                user.Passwordhash = newPassword;  // Assuming the password is already hashed
                _context.SaveChanges();
            }
        }
    }
}
