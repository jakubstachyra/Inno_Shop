using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Interfaces;

namespace UserManagement.Infrastructure.Repositories
{
    public class UserRepository(ApplicationDbContext context) : Repository<User>(context), IUserRepository
    {
        public async Task SoftDeleteAsync(int userId)
        {
            var user = await context.Users.FindAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            user.IsDeleted = true;
            await context.SaveChangesAsync();
        }

    }
}
