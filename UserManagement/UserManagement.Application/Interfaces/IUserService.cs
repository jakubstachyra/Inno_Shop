using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.DTOs;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Interfaces
{
    public interface IUserService
    {
        Task<User> RegisterUserAsync(RegisterUserDto registerDto);
        Task<bool> ConfirmEmailAsync(string token);

        Task<string> AuthenticateAndGenerateTokenAsync(string email, string password);
    }
}
