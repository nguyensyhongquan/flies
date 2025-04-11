using FliesProject.Models.Entities;

namespace FliesProject.Repositories.IGenericRepository
{
    public interface IStudentRepository
    {
        Task<IEnumerable<User>> GetAllStudent();
        Task<bool> IsEmailExist(string email);
        Task<bool> IsUsernameExist(string username);
        Task<bool> IsPhoneNumberExist(string phoneNumber);
    }
}
