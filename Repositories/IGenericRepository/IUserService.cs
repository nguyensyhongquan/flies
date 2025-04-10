using FliesProject.Models.Entities;

namespace FliesProject.Repositories.IGenericRepository
{
    public interface IUserService
    {
        User GetUserByUsername(string username);
    }
}
