using FliesProject.Data;
using FliesProject.Models.Entities;

namespace FliesProject.Service
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
    }
}
