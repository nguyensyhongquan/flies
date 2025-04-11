using FliesProject.Models.Entities;

namespace FliesProject.Service
{
    public interface IUserService
    {
        User GetUserByUsername(string username);
    }
}
