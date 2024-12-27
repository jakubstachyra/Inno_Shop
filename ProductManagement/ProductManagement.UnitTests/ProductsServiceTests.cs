using Moq;
using ProductService.Application.Interfaces;
using ProductService.Application.Services;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;

namespace ProductService.Tests
{
    [TestFixture]
    public class ProductsServiceTests
    {
        private Mock<IProductRepository> _productRepositoryMock;
        private IProductService _productService;

        [SetUp]
        public void SetUp()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _productService = new ProductsService(_productRepositoryMock.Object);
        }

        [Test]
        public async Task GetAllProductsForUserAsync_ShouldReturnProductsForUser()
        {
            var userId = 1;
            var products = new List<Product>
            {
                new Product { ID = 1, Name = "Product1", CreatorUserID = userId },
                new Product { ID = 2, Name = "Product2", CreatorUserID = userId }
            };

            _productRepositoryMock
                .Setup(repo => repo.GetAllByUserIdAsync(userId))
                .ReturnsAsync(products);

            var result = await _productService.GetAllProductsForUserAsync(userId);

            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().Name, Is.EqualTo("Product1"));
        }

        [Test]
        public async Task GetProductByIdAsync_ShouldReturnProduct_WhenProductExistsAndBelongsToUser()
        {
            var userId = 1;
            var product = new Product { ID = 1, Name = "Product1", CreatorUserID = userId };

            _productRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(product);

            var result = await _productService.GetProductByIdAsync(1, userId);

            Assert.IsNotNull(result);
            Assert.That(result.Name, Is.EqualTo("Product1"));
        }

        [Test]
        public async Task GetProductByIdAsync_ShouldReturnNull_WhenProductDoesNotBelongToUser()
        {
            var userId = 1;
            var product = new Product { ID = 1, Name = "Product1", CreatorUserID = 2 };

            _productRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(product);

            var result = await _productService.GetProductByIdAsync(1, userId);

            Assert.IsNull(result);
        }

        [Test]
        public async Task AddProductAsync_ShouldSetCreationDateAndCallRepository()
        {
            var product = new Product { Name = "NewProduct", CreatorUserID = 1 };

            await _productService.AddProductAsync(product);

            _productRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Product>(p =>
                p.Name == "NewProduct" &&
                p.CreatorUserID == 1 &&
                p.CreationDate != default)), Times.Once);
        }

        [Test]
        public void UpdateProductAsync_ShouldThrowUnauthorizedAccessException_WhenProductDoesNotBelongToUser()
        {
 
            var userId = 1;
            var productToUpdate = new Product { ID = 1, Name = "UpdatedProduct", CreatorUserID = 1 };
            var existingProduct = new Product { ID = 1, Name = "ExistingProduct", CreatorUserID = 2 };

            _productRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(existingProduct);

            Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _productService.UpdateProductAsync(productToUpdate, userId));
        }

        [Test]
        public async Task DeleteProductAsync_ShouldMarkProductAsDeleted()
        {
            var userId = 1;
            var product = new Product { ID = 1, Name = "ProductToDelete", CreatorUserID = userId };

            _productRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(product);

            await _productService.DeleteProductAsync(1, userId);

            _productRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Product>(p =>
                p.IsDeleted == true)), Times.Once);
        }

        [Test]
        public async Task SearchProductsAsync_ShouldFilterProductsBySearchQuery()
        {
            var userId = 1;
            var products = new List<Product>
            {
                new Product { ID = 1, Name = "Laptop", CreatorUserID = userId },
                new Product { ID = 2, Name = "Mouse", CreatorUserID = userId }
            };

            _productRepositoryMock
                .Setup(repo => repo.GetAllByUserIdAsync(userId))
                .ReturnsAsync(products);

            var result = await _productService.SearchProductsAsync(userId, "Lap", null, null, null);

            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("Laptop"));
        }
    }
}
