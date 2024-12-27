using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UserManagement.Application.DTOs;
using UserManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.Application.Interfaces;
using Moq;

namespace UserManagement.IntegrationTests
{
    [TestFixture]
    public class UsersControllerTests
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;

        [SetUp]
        public void SetUp()
        {
            var mockEmailService = new Mock<IEmailService>();

            mockEmailService.Setup(service => service.SendEmailAsync(It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        services.AddDbContext<ApplicationDbContext>(options =>
                        {
                            options.UseInMemoryDatabase("InMemoryDbForTesting");
                        });

                        var emailServiceDescriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(IEmailService));
                        if (emailServiceDescriptor != null)
                        {
                            services.Remove(emailServiceDescriptor);
                        }
                        services.AddSingleton(mockEmailService.Object);
                    });
                });

            _client = _factory.CreateClient();
        }


        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Test]
        public async Task Register_ShouldReturnCreatedStatus_WhenUserIsValid()
        {
            var registerDto = new RegisterUserDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "SecurePassword123"
            };

            var response = await _client.PostAsJsonAsync("/api/users/register", registerDto);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }

        [Test]
        public async Task Register_ShouldReturnConflict_WhenEmailAlreadyExists()
        {
            var registerDto = new RegisterUserDto
            {
                Name = "Test User",
                Email = "duplicate@example.com",
                Password = "SecurePassword123"
            };

            await _client.PostAsJsonAsync("/api/users/register", registerDto);

            var response = await _client.PostAsJsonAsync("/api/users/register", registerDto);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task Login_ShouldReturnOk_WhenCredentialsAreValid()
        {
            var registerDto = new RegisterUserDto
            {
                Name = "Test User",
                Email = "login@example.com",
                Password = "SecurePassword123"
            };

            await _client.PostAsJsonAsync("/api/users/register", registerDto);

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var user = dbContext.Users.FirstOrDefault(u => u.Email == "login@example.com");
                Assert.IsNotNull(user);

                user.IsActive = true; // Aktywacja u¿ytkownika
                dbContext.SaveChanges();
            }

            var loginDto = new LoginUserDto
            {
                Email = "login@example.com",
                Password = "SecurePassword123"
            };

            var response = await _client.PostAsJsonAsync("/api/users/login", loginDto);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await response.Content.ReadAsStringAsync();
            var jsonElement = System.Text.Json.JsonDocument.Parse(content).RootElement;
            var token = jsonElement.GetProperty("token").GetString();

            Assert.IsNotNull(token);
        }


        [Test]
        public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
        {
            var loginDto = new LoginUserDto
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword123"
            };

            var response = await _client.PostAsJsonAsync("/api/users/login", loginDto);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task GetUserById_ShouldReturnUser_WhenUserExists()
        {
            var registerDto = new RegisterUserDto
            {
                Name = "Test User",
                Email = "userbyid@example.com",
                Password = "SecurePassword123"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/users/register", registerDto);
            Assert.That(registerResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            int userId;
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var user = dbContext.Users.FirstOrDefault(u => u.Email == "userbyid@example.com");
                Assert.IsNotNull(user);

                userId = user.ID;
            }

            var response = await _client.GetAsync($"/api/users/{userId}");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await response.Content.ReadAsStringAsync();
            var jsonElement = System.Text.Json.JsonDocument.Parse(content).RootElement;
            var name = jsonElement.GetProperty("name").GetString();

            Assert.That(name, Is.EqualTo("Test User"));
        }


        [Test]
        public async Task SoftDeleteUser_ShouldMarkUserAsDeleted()
        {

            var registerDto = new RegisterUserDto
            {
                Name = "Test User",
                Email = "delete@example.com",
                Password = "SecurePassword123"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/users/register", registerDto);
            Assert.That(registerResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));


            int userId;
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var user = dbContext.Users.IgnoreQueryFilters().FirstOrDefault(u => u.Email == "delete@example.com");
                Assert.IsNotNull(user, "User should exist in the database");

                userId = user.ID;
            }

            var deleteResponse = await _client.DeleteAsync($"/api/users/{userId}");
            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var deletedUser = dbContext.Users.IgnoreQueryFilters().FirstOrDefault(u => u.ID == userId);
                Assert.IsNotNull(deletedUser, "User should exist in the database after soft delete");
                Assert.IsTrue(deletedUser!.IsDeleted, "User should be marked as deleted");
            }
        }


    }
}
