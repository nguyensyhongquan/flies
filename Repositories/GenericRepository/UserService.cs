using FliesProject.Data;
using FliesProject.Models.Entities;
using FliesProject.Repositories.IGenericRepository;

namespace FliesProject.Repositories.GenericRepository
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
