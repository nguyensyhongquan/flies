using FliesProject.Models.Entities;

namespace FliesProject.Services
{
    public interface IUserService
    {
        User GetUserByUsername(string username);
        User GetUserByEmail(string email);
        void UpdatePassword(string userId, string newPassword);
        User GetUserById(string userId);
    }
}
