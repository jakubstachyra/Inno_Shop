using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Interfaces
{
    public interface IUserRepository : IRepository<User> {
        Task SoftDeleteAsync(int userId);
    }

}
