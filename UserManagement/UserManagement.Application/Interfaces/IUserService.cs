using UserManagement.Domain.Entities;

namespace UserManagement.Application.Interfaces
{
    public interface IUserService
    {
        Task<User> RegisterUserAsync(string name, string email, string password);
    }
}
