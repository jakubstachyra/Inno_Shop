using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ProductService.Infrastructure.Data;
using ProductService.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;

namespace ProductService.IntegrationTests
{
    [TestFixture]
    public class ProductsControllerTests
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;
        [SetUp]
        public void SetUp()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Testing");

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
                    });
                });

            _client = _factory.CreateClient();

            var token = GenerateJwtToken();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Wyczyœæ dane w bazie przed ka¿dym testem
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Products.RemoveRange(dbContext.Products);
                dbContext.SaveChanges();
            }
        }


        private string GenerateJwtToken()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("UserManagmentService213qweds89y0n2uasdf")); 
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "https://localhost:5000",
                audience: "https://localhost:5001",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [TearDown]
        public void TearDown()
        {
            _factory.Dispose();
            _client.Dispose();
        }

        [Test]
        public async Task Create_ShouldReturnCreated_WhenProductIsValid()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Description = "A sample product",
                Price = 50,
                IsAvailable = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/products", product);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            // Sprawdzenie w bazie danych
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var createdProduct = dbContext.Products.FirstOrDefault(p => p.Name == "Test Product");
                Assert.IsNotNull(createdProduct);
            }
        }


        [Test]
        public async Task GetAll_ShouldReturnProducts_ForAuthenticatedUser()
        {
            // Arrange: Dodaj produkt do bazy
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Products.Add(new Product
                {
                    Name = "Test Product",
                    Description = "A sample product",
                    Price = 50,
                    IsAvailable = true,
                    CreatorUserID = 1
                });
                dbContext.SaveChanges();
            }

            // Act
            var response = await _client.GetAsync("/api/products");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var products = await response.Content.ReadFromJsonAsync<Product[]>();
            Assert.IsNotNull(products);
            Assert.That(products.Length, Is.EqualTo(1));
            Assert.That(products[0].Name, Is.EqualTo("Test Product"));
        }

        [Test]
        public async Task Delete_ShouldRemoveProduct_WhenProductExists()
        {
            // Arrange: Dodaj produkt do bazy
            int productId;
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var product = new Product
                {
                    Name = "Product to Delete",
                    Description = "A product for deletion",
                    Price = 30,
                    IsAvailable = true,
                    CreatorUserID = 1
                };
                dbContext.Products.Add(product);
                dbContext.SaveChanges();

                productId = product.ID;
            }

            // Act
            var response = await _client.DeleteAsync($"/api/products/{productId}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            // SprawdŸ, czy produkt zosta³ usuniêty
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var deletedProduct = dbContext.Products.FirstOrDefault(p => p.ID == productId);
                Assert.IsNull(deletedProduct);
            }
        }

        [Test]
        public async Task Search_ShouldReturnFilteredProducts()
        {
            // Arrange: Dodaj produkty do bazy
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Products.AddRange(
                    new Product
                    {
                        Name = "Cheap Product",
                        Description = "Low price",
                        Price = 10,
                        IsAvailable = true,
                        CreatorUserID = 1
                    },
                    new Product
                    {
                        Name = "Expensive Product",
                        Description = "High price",
                        Price = 100,
                        IsAvailable = true,
                        CreatorUserID = 1
                    });
                dbContext.SaveChanges();
            }

            // Act
            var response = await _client.GetAsync("/api/products/search?minPrice=50");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var products = await response.Content.ReadFromJsonAsync<Product[]>();
            Assert.IsNotNull(products);
            Assert.That(products.Length, Is.EqualTo(1));
            Assert.That(products[0].Name, Is.EqualTo("Expensive Product"));
        }
    }
}
