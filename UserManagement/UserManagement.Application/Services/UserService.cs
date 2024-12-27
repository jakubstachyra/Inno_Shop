using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserManagement.Application.DTOs;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Interfaces;

namespace UserManagement.Application.Services;

public class UserService(IUserRepository userRepository,
    IConfiguration configuration, IEmailService emailService): IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IConfiguration _configuration = configuration;
    private readonly IEmailService _emailService = emailService;
    public async Task<User> RegisterUserAsync(RegisterUserDto registerUserDto)
    {
        var users = await _userRepository.GetAllAsync(); 
        var userFound = users.Where(u => u.Email == registerUserDto.Email).FirstOrDefault();

        if (userFound != null)
        {
            throw new InvalidOperationException("Email already exists.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerUserDto.Password);

        var activationToken = GenerateConfirmationToken();

        var user = new User
        {
            Name = registerUserDto.Name,
            Email = registerUserDto.Email,
            PasswordHash = passwordHash,
            Role = "User",
            IsActive = false,
            ActivationToken = activationToken
        };


        var confirmationLink = $"https://localhost:7107/api/user/confirm-email?token={activationToken}";
        await _emailService.SendEmailAsync(
            user.Email,
            "Confirm your account",
            $"Please confirm your account by clicking <a href=\"{confirmationLink}\">here</a>."
        );
        await _userRepository.AddAsync(user);

        return user;
    }

    private string GenerateConfirmationToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }

    public async Task<string> AuthenticateAndGenerateTokenAsync(string email, string password)
    {
        var users = await _userRepository.GetAllAsync();
        var user = users.Where(u => u.Email == email).FirstOrDefault();

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return null!;
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("Account is not confirmed. Please check your email to confirm your account.");
        }

        var token = GenerateJwtToken(user);
        return token;
    }

    public string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.ID.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "UserManagmentService",
            audience: "ProductManagerService",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
            );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public async Task<bool> ConfirmEmailAsync(string token)
    {
        var users = await _userRepository.GetAllAsync();
        var user =   users .Where(u => u.ActivationToken == token).FirstOrDefault();

        if (user == null || user.IsActive)
        {
            return false; 
        }

        user.IsActive = true;
        user.ActivationToken = null!;
        await _userRepository.UpdateAsync(user);

        return true;
    }
    public async Task UpdateUserAsync(int userId, UpdateUserDto updateUserDto)
    {
        var users = await _userRepository.GetAllAsync();
        var user =  users.Where(u => u.ID == userId && !u.IsDeleted)
            .FirstOrDefault();

        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        user.Name = updateUserDto.Name ?? user.Name;
        user.Email = updateUserDto.Email ?? user.Email;

        await _userRepository.UpdateAsync(user);
    }
    public async Task<User> GetUserByIdAsync(int userId)
    {
        var users = await _userRepository.GetAllAsync();
        var user = users.Where(u => u.ID == userId && !u.IsDeleted)
            .FirstOrDefault();

        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        user.PasswordHash = null!;

        return user;
    }

    public async Task SoftDeleteUserAsync(int userId)
    {
        await _userRepository.SoftDeleteAsync(userId);
    }
}
