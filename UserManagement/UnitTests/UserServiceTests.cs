using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.DTOs;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Interfaces;

namespace UserManagement.Tests
{
    [TestFixture]
    public class UserServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<IEmailService> _emailServiceMock;
        private Mock<IConfiguration> _configurationMock;
        private IUserService _userService;

        [SetUp]
        public void SetUp()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _emailServiceMock = new Mock<IEmailService>();
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock.Setup(c => c["JWT:Key"]).Returns("YourSuperSecretKeyasdfjky3298prweybaudshaz");
            _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("https://localhost:5000");
            _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("https://localhost:5001");

            _userService = new UserService(
                _userRepositoryMock.Object,
                _configurationMock.Object,
                _emailServiceMock.Object);
        }

        [Test]
        public async Task RegisterUserAsync_ShouldRegisterNewUser()
        {
            var registerDto = new RegisterUserDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "SecurePassword123"
            };

            _userRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<User>());
            var result = await _userService.RegisterUserAsync(registerDto);

            Assert.IsNotNull(result);
            Assert.That(result.Name, Is.EqualTo(registerDto.Name));
            Assert.That(result.Email, Is.EqualTo(registerDto.Email));
            _emailServiceMock.Verify(email =>
                email.SendEmailAsync(
                    result.Email,
                    "Confirm your account",
                    It.IsAny<string>()), Times.Once);
            _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Test]
        public void RegisterUserAsync_ShouldThrowException_WhenEmailAlreadyExists()
        {
            var registerDto = new RegisterUserDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "SecurePassword123"
            };

            _userRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<User>
                {
                    new User {
                        Email = "test@example.com",
                        Name = "user",
                        Role = "user",
                        IsActive = false,
                        PasswordHash = "asdwaesd"
                        
                    }
                }); ;

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userService.RegisterUserAsync(registerDto));
        }

        [Test]
        public async Task AuthenticateAndGenerateTokenAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            var user = new User
            {
                ID = 1,
                Email = "test@example.com",
                Name = "John",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("SecurePassword123"),
                IsActive = true,
                Role = "User"
            };

            _userRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<User> { user });

            var token = await _userService.AuthenticateAndGenerateTokenAsync("test@example.com", "SecurePassword123");

            Assert.IsNotNull(token);
        }

        [Test]
        public void AuthenticateAndGenerateTokenAsync_ShouldThrowException_WhenAccountNotConfirmed()
        {
            var user = new User
            {
                ID = 1,
                Email = "test@example.com",
                Name = "Joe",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("SecurePassword123"),
                IsActive = false,
                Role = "user"
            };

            _userRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<User> { user });

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userService.AuthenticateAndGenerateTokenAsync("test@example.com", "SecurePassword123"));
        }

        [Test]
        public async Task ConfirmEmailAsync_ShouldActivateAccount()
        {
            var token = "activation-token";
            var user = new User
            {
                ID = 1,
                Email = "test@example.com",
                Name = "Cody",
                PasswordHash = "asdfew3",
                IsActive = false,
                ActivationToken = token,
                Role = "user"
            };

            _userRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<User> { user });

            var result = await _userService.ConfirmEmailAsync(token);

            Assert.IsTrue(result);
            _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<User>(u => u.IsActive && u.ActivationToken == null)), Times.Once);
        }

        [Test]
        public async Task UpdateUserAsync_ShouldUpdateUserDetails()
        {
            var userId = 1;
            var updateDto = new UpdateUserDto { Name = "Updated Name" };
            var user = new User { 
                ID = userId, 
                Name = "Old Name",
                Email = "test@example.com",
                IsActive= true,
                PasswordHash = "Asdr3eqwsda##21",
                Role = "user" };

            _userRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<User> { user });

            await _userService.UpdateUserAsync(userId, updateDto);

            _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<User>(u =>
                u.ID == userId &&
                u.Name == "Updated Name" &&
                u.Email == "test@example.com")), Times.Once);
        }

        [Test]
        public void UpdateUserAsync_ShouldThrowException_WhenUserNotFound()
        {
            var userId = 1;
            var updateDto = new UpdateUserDto { Name = "Updated Name" };

            _userRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<User>());

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userService.UpdateUserAsync(userId, updateDto));
        }

        [Test]
        public async Task SoftDeleteUserAsync_ShouldMarkUserAsDeleted()
        {
            var userId = 1;

            await _userService.SoftDeleteUserAsync(userId);

            _userRepositoryMock.Verify(repo => repo.SoftDeleteAsync(userId), Times.Once);
        }
    }
}
