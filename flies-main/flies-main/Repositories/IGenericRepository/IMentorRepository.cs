using FliesProject.Models.Entities;

namespace FliesProject.Repositories.IGenericRepository
{
    public interface IMentorRepository
    {
        Task<IEnumerable<User>> GetAllMentor();
        Task<bool> IsEmailExist(string email);
        Task<bool> IsUsernameExist(string username);
        Task<bool> IsPhoneNumberExist(string phoneNumber);
    }
}
