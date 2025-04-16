using FliesProject.Models.Entities;

namespace FliesProject.Services
{
    public interface IUserService
    {
        User GetUserByUsername(string username);
    }
}
