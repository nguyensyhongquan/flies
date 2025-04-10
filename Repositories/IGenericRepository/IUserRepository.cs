using FliesProject.Models.Entities;

namespace FliesProject.Repositories.IGenericRepository
{
    public interface IUserRepository
    {
     
        Task<IEnumerable<User>> GetAllUsersAsync();

  
        Task<User> GetUserByIdAsync(int id);

     
        Task<User> GetUserByEmailAsync(string email);

  
        Task<User> CreateUserAsync(User user);

        Task UpdateUserAsync(User user);

   
        Task DeleteUserAsync(int id);
    }
}
