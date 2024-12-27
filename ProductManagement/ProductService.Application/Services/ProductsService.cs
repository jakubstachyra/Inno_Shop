using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;

namespace ProductService.Application.Services
{
        public class ProductsService : IProductService
        {
            private readonly IProductRepository _productRepository;

            public ProductsService(IProductRepository productRepository)
            {
                _productRepository = productRepository;
            }

            public async Task<IEnumerable<Product>> GetAllProductsForUserAsync(int userId)
            {
                return await _productRepository.GetAllByUserIdAsync(userId);
            }

            public async Task<Product?> GetProductByIdAsync(int id, int userId)
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null || product.CreatorUserID != userId)
                {
                    return null;
                }

                return product;
            }

            public async Task AddProductAsync(Product product)
            {
                product.CreationDate = DateTime.UtcNow;

                await _productRepository.AddAsync(product);
            }

            public async Task UpdateProductAsync(Product product, int userId)
            {
                var existingProduct = await _productRepository.GetByIdAsync(product.ID);
                if (existingProduct == null || existingProduct.CreatorUserID != userId)
                {
                    throw new UnauthorizedAccessException("You cannot edit this product.");
                }

                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.IsAvailable = product.IsAvailable;

                await _productRepository.UpdateAsync(existingProduct);
            }

            public async Task DeleteProductAsync(int id, int userId)
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null || product.CreatorUserID != userId)
                {
                    throw new UnauthorizedAccessException("You cannot delete this product.");
                }

                product.IsDeleted = true;
                await _productRepository.UpdateAsync(product);
            }
            public async Task<IEnumerable<Product>> SearchProductsAsync(int userId,
                string? searchQuery, decimal? minPrice, decimal? maxPrice, bool? isAvailable)
            {
                var products = await _productRepository.GetAllByUserIdAsync(userId);

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    products = products.Where(p => p.Name.
                    Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                        || p.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase));
                }

                if (minPrice.HasValue)
                {
                    products = products.Where(p => p.Price >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    products = products.Where(p => p.Price <= maxPrice.Value);
                }

                if (isAvailable.HasValue)
                {
                    products = products.Where(p => p.IsAvailable == isAvailable.Value);
                }

                return products;
            }


    }

}

