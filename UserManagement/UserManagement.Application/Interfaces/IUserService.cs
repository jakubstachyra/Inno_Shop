using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UserManagement.Application.DTOs;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Interfaces
{
    public interface IUserService
    {
        Task<User> RegisterUserAsync(RegisterUserDto registerDto);
        Task<bool> ConfirmEmailAsync(string token);

        Task<string> AuthenticateAndGenerateTokenAsync(string email, string password);
        Task<User> GetUserByIdAsync(int userId);
        Task SoftDeleteUserAsync(int userId);
        Task UpdateUserAsync(int userId, UpdateUserDto updateUserDto);

    }
}
