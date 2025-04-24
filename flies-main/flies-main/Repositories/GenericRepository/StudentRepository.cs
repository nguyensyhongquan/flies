using FliesProject.Data;
using FliesProject.Models.Entities;
using FliesProject.Repositories.IGenericRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FliesProject.Repositories.GenericRepository
{
    public class StudentRepository:IStudentRepository 
    {
        private readonly FiliesContext _context;
        public StudentRepository(FiliesContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<User>> GetAllStudent()
        {
            return await _context.Users.Where(u => u.Role == "Student").ToListAsync();
        }


        /// <summary>
        /// Kiểm tra xem email đã tồn tại trong cơ sở dữ liệu hay chưa.
        /// </summary>

        /// <returns></returns>
        public async Task<bool> IsEmailExist(string email)
        {
            bool isExist = await _context.Users.AnyAsync(u => u.Email == email);
            return isExist;
        }

        public async Task<bool> IsPhoneNumberExist(string phoneNumber)
        {
            bool isExist = await _context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
            return isExist;
        }

        public async Task<bool> IsUsernameExist(string username)
        {
            bool isExist = await _context.Users.AnyAsync(u => u.Username == username);
            return isExist;
        }
    }
}
