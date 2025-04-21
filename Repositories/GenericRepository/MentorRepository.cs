using FliesProject.Data;
using FliesProject.Models.Entities;
using FliesProject.Repositories.IGenericRepository;
using Microsoft.EntityFrameworkCore;

namespace FliesProject.Repositories.GenericRepository
{
    public class MentorRepository : IMentorRepository
    {
        private readonly FiliesContext _context;
        public MentorRepository(FiliesContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<User>> GetAllMentor()
        {
            return await _context.Users.Where(u => u.Role == "Mentor").ToListAsync();
        }

        public Task<bool> IsEmailExist(string email)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsPhoneNumberExist(string phoneNumber)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsUsernameExist(string username)
        {
            throw new NotImplementedException();
        }
    }
}
