using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Interfaces;

namespace UserManagement.Application.Services;

public class UserService(IUserRepository userRepository): IUserService
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<User> RegisterUserAsync(string name, string email, string password)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = passwordHash,
            Role = "User",
            IsActive = false
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();
        return user;
    }
}
